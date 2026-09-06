using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Domain.Policies;

public static class Rules
{
    public static void Require(bool valid, string message, string field = "request")
    {
        if (!valid)
            throw new StudioException(400, message, field);
    }

    public static decimal Round(decimal amount) =>
        decimal.Round(amount, 2, MidpointRounding.AwayFromZero);

    public static readonly TimeZoneInfo Toronto = TimeZoneInfo.FindSystemTimeZoneById(
        "America/Toronto"
    );

    public static DateTimeOffset Local(DateTimeOffset instant) =>
        TimeZoneInfo.ConvertTime(instant, Toronto);

    public static void Interval(DateTimeOffset start, DateTimeOffset end)
    {
        Require(end > start, "End must be after start.", "endsAt");
        foreach (var value in new[] { start, end })
            Require(
                value.Minute % 15 == 0
                    && value.Second == 0
                    && value.Ticks % TimeSpan.TicksPerSecond == 0
                    && Toronto.GetUtcOffset(value) == value.Offset,
                "Use a valid Toronto time and offset on a 15-minute boundary.",
                "startsAt"
            );
    }

    public static bool Overlaps(
        DateTimeOffset a,
        DateTimeOffset b,
        DateTimeOffset c,
        DateTimeOffset d
    ) => a < d && c < b;

    public static void Text(string? value, string field, int max = 1000) =>
        Require(
            !string.IsNullOrWhiteSpace(value) && value.Length <= max,
            $"{field} is required and must be at most {max} characters.",
            field
        );
}
