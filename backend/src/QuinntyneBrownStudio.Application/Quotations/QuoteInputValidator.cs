using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Exceptions;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.Policies;

namespace QuinntyneBrownStudio.Application.Quotations;

public static class QuoteInputValidator
{
    public static void Validate(QuoteInput q, DateTimeOffset now)
    {
        Rules.Interval(q.StartsAt, q.EndsAt);
        Rules.Require(Enum.IsDefined(q.Service), "Select a supported service.");
        Rules.Require(
            Rules.Local(q.StartsAt).Date >= Rules.Local(now).Date,
            "Session date cannot be in the past.",
            "startsAt"
        );
        Rules.Require(
            q.Locations is { Length: > 0 and <= 100 },
            "Provide between 1 and 100 locations.",
            "locations"
        );
        Rules.Require(
            q.AssistantCount >= 0 && q.EquipmentUnits >= 0 && q.LunchCount >= 0,
            "Counts must be nonnegative."
        );
        foreach (var l in q.Locations)
        {
            if (l?.Location is null)
                throw new StudioException(400, "Resolve every location before calculating.", "locations");
            AdminCatalog.ValidateLocation(l.Location);
            Rules.Require(
                l.ParkingAmount >= 0 && l.StudioHours >= 0 && l.StudioHours % 0.25m == 0,
                "Location costs must be nonnegative and studio hours use quarter hours."
            );
        }
    }
}
