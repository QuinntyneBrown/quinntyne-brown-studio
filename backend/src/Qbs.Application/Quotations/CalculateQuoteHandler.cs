using MediatR;
using Qbs.Domain;

namespace Qbs.Application;

public sealed class CalculateQuoteHandler(
    IStudioStore store,
    IRouteDistanceService routes,
    IClock clock
) : IRequestHandler<CalculateQuote, QuoteResult>
{
    public Task<QuoteResult> Handle(CalculateQuote request, CancellationToken ct) =>
        store.Run(
            "pricing",
            async tx =>
            {
                var q = request.Input;
                Rules.Interval(q.StartsAt, q.EndsAt);
                Rules.Require(Enum.IsDefined(q.Service), "Select a supported service.");
                Rules.Require(
                    Rules.Local(q.StartsAt).Date >= Rules.Local(clock.UtcNow).Date,
                    "Session date cannot be in the past.",
                    "startsAt"
                );
                Rules.Require(
                    q.Locations.Length > 0 && q.Locations.Length <= 100,
                    "Provide between 1 and 100 locations.",
                    "locations"
                );
                Rules.Require(
                    q.AssistantCount >= 0 && q.EquipmentUnits >= 0 && q.LunchCount >= 0,
                    "Counts must be nonnegative."
                );
                foreach (var l in q.Locations)
                {
                    AdminCatalog.ValidateLocation(l.Location);
                    Rules.Require(
                        l.ParkingAmount >= 0 && l.StudioHours >= 0 && l.StudioHours % 0.25m == 0,
                        "Location costs must be nonnegative and studio hours use quarter hours."
                    );
                }
                var rates = await tx.Get<RateConfiguration>(AdminCatalog.ConfigurationId);
                var studios = await tx.List<Studio>();
                var baseStudio = studios.SingleOrDefault(x => x.IsBase);
                if (rates == null || baseStudio == null)
                    throw new StudioException(
                        503,
                        "Configure quotation rates and a studio base before calculating."
                    );
                decimal Rate(string key) =>
                    rates.CostRates.GetValueOrDefault(key)
                    ?? throw new StudioException(503, "A required rate is not configured.");
                var serviceRate =
                    rates.ServiceRates.GetValueOrDefault(q.Service)
                    ?? throw new StudioException(503, "The photography rate is not configured.");
                var km =
                    await routes.Metres(
                        new[] { baseStudio.ResolvedAddress }
                            .Concat(q.Locations.Select(x => x.Location))
                            .Append(baseStudio.ResolvedAddress)
                            .ToArray(),
                        ct
                    ) / 1000m;
                var lines = new List<QuoteLine>();
                void Add(string kind, decimal count, decimal rate, int? location = null) =>
                    lines.Add(
                        new(
                            kind,
                            location,
                            count,
                            new(rate),
                            new(Rules.Round(checked(count * rate)))
                        )
                    );
                var hours = (decimal)(q.EndsAt - q.StartsAt).TotalMinutes / 60;
                Add("photography", hours, serviceRate);
                Add("travel", km, Rate("travel"));
                Add("equipment", q.EquipmentUnits, Rate("equipment"));
                Add("lunch", q.LunchCount, Rate("lunch"));
                Add("assistant", q.AssistantCount * hours, Rate("assistant"));
                for (var i = 0; i < q.Locations.Length; i++)
                {
                    var l = q.Locations[i];
                    Add("parking", 1, l.ParkingAmount, i);
                    if (l.StudioId != null)
                    {
                        var studio =
                            studios.SingleOrDefault(x => x.Id == l.StudioId && x.Enabled)
                            ?? throw new StudioException(
                                400,
                                "Select an enabled studio.",
                                "studioId"
                            );
                        Add("studio", l.StudioHours, studio.HourlyFee, i);
                    }
                }
                var subtotal = lines.Sum(x => x.Amount.Amount);
                var config =
                    await tx.Get<DiscountConfiguration>(AdminCatalog.ConfigurationId) ?? new();
                var discount = DiscountPolicy.Calculate(
                    config,
                    subtotal,
                    q.Code,
                    q.StartsAt,
                    clock.UtcNow
                );
                var revision = await tx.Get<ConfigurationRevision>(AdminCatalog.ConfigurationId);
                return new QuoteResult(
                    q.InputRevision,
                    revision?.Revision ?? 0,
                    lines.ToArray(),
                    new(subtotal),
                    discount,
                    new(subtotal - discount.Amount.Amount),
                    await Scheduling.Availability(tx, q.StartsAt, q.EndsAt, q.PhotographerId)
                );
            },
            ct
        );
}
