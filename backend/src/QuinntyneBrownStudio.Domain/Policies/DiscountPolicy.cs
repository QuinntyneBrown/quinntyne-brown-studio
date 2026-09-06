using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;

namespace QuinntyneBrownStudio.Domain.Policies;

public static class DiscountPolicy
{
    public static AppliedDiscount Calculate(
        DiscountConfiguration d,
        decimal subtotal,
        string? code,
        DateTimeOffset session,
        DateTimeOffset now
    )
    {
        var today = DateOnly.FromDateTime(Rules.Local(now).Date);
        var date = DateOnly.FromDateTime(Rules.Local(session).Date);
        var candidates = new List<(DiscountRule Rule, string Kind)>();
        string? error = null;
        if (!string.IsNullOrWhiteSpace(code))
        {
            var found = d.CodeRules.FirstOrDefault(x =>
                string.Equals(x.Code?.Trim(), code.Trim(), StringComparison.OrdinalIgnoreCase)
            );
            if (
                found is { Enabled: true }
                && (found.ValidFrom == null || today >= found.ValidFrom)
                && (found.ValidTo == null || today <= found.ValidTo)
            )
                candidates.Add((found, "Code"));
            else
                error = "This code cannot be applied.";
        }
        if (d.AdvanceRule.Enabled && date.DayNumber - today.DayNumber >= d.AdvanceRule.Threshold)
            candidates.Add((d.AdvanceRule, "Advance"));
        if (d.WeekdayRule.Enabled && d.WeekdayRule.Weekdays.Contains(date.DayOfWeek))
            candidates.Add((d.WeekdayRule, "Weekday"));
        var best = candidates.OrderByDescending(x => x.Rule.Percentage).FirstOrDefault();
        return best.Rule == null
            ? new(null, null, 0, new(0), error)
            : new(
                best.Rule.Id,
                best.Kind,
                best.Rule.Percentage,
                new(Rules.Round(subtotal * best.Rule.Percentage / 100)),
                error
            );
    }
}
