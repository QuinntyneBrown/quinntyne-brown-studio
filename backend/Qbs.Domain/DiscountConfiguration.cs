namespace Qbs.Domain;

public sealed class DiscountConfiguration : Entity
{
    public DiscountRule AdvanceRule { get; set; } = new() { Threshold = 90 };
    public DiscountRule WeekdayRule { get; set; } = new();
    public DiscountRule[] CodeRules { get; set; } = [];
}
