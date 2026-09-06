using Qbs.Domain;

namespace Qbs.Application;

public sealed class Scheduling(IStudioStore store)
{
    public Task<AvailabilityResult> Check(
        DateTimeOffset start,
        DateTimeOffset end,
        Guid? photographer
    ) => store.Run("availability", tx => Availability(tx, start, end, photographer));

    public Task<PhotographerSchedule> Save(Guid id, PhotographerSchedule input) =>
        store.Run(
            "photographer:" + id,
            async tx =>
            {
                if (await tx.Get<Photographer>(id) == null)
                    throw new StudioException(404, "Photographer not found.");
                Rules.Require(
                    input.Buffers.Before >= 0
                        && input.Buffers.After >= 0
                        && input.Buffers.Before % 15 == 0
                        && input.Buffers.After % 15 == 0,
                    "Buffers must be nonnegative quarter-hour increments."
                );
                foreach (var window in input.WorkingWindows.Concat(input.UnavailableWindows))
                    Rules.Interval(window.StartsAt, window.EndsAt);
                input.Id = id;
                input.PhotographerId = id;
                var sessions = (await tx.List<Session>())
                    .Where(x => x.PhotographerId == id)
                    .ToArray();
                foreach (var session in sessions)
                    if (
                        !Fits(
                            input,
                            session.StartsAt,
                            session.EndsAt,
                            sessions.Where(x => x.Id != session.Id)
                        )
                    )
                        throw new StudioException(
                            409,
                            "Schedule conflicts with an assigned session."
                        );
                await tx.Save(input, input.ExpectedVersion);
                return input;
            }
        );

    public static async Task<AvailabilityResult> Availability(
        IStudioTransaction tx,
        DateTimeOffset start,
        DateTimeOffset end,
        Guid? selected,
        Guid? excluding = null
    )
    {
        Rules.Interval(start, end);
        var eligible = new List<Guid>();
        var sessions = await tx.List<Session>();
        foreach (
            var person in (await tx.List<Photographer>()).Where(x =>
                x.Active && (selected == null || x.Id == selected)
            )
        )
        {
            var schedule = await tx.Get<PhotographerSchedule>(person.Id);
            if (
                schedule != null
                && Fits(
                    schedule,
                    start,
                    end,
                    sessions.Where(x => x.PhotographerId == person.Id && x.Id != excluding)
                )
            )
                eligible.Add(person.Id);
        }
        return new(
            start,
            end,
            eligible.Count > 0,
            eligible.ToArray(),
            eligible.Count == 0 ? "NoAvailablePhotographer" : null
        );
    }

    private static bool Fits(
        PhotographerSchedule s,
        DateTimeOffset start,
        DateTimeOffset end,
        IEnumerable<Session> commitments
    )
    {
        var a = start.AddMinutes(-s.Buffers.Before);
        var b = end.AddMinutes(s.Buffers.After);
        return s.WorkingWindows.Any(x => x.StartsAt <= a && x.EndsAt >= b)
            && !s.UnavailableWindows.Any(x => Rules.Overlaps(a, b, x.StartsAt, x.EndsAt))
            && !commitments.Any(x =>
                Rules.Overlaps(
                    a,
                    b,
                    x.StartsAt.AddMinutes(-s.Buffers.Before),
                    x.EndsAt.AddMinutes(s.Buffers.After)
                )
            );
    }
}
