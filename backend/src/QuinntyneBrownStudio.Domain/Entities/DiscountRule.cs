namespace QuinntyneBrownStudio.Domain.Entities;

public sealed class DiscountRule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public bool Enabled { get; set; }
    public decimal Percentage { get; set; }
    public int Threshold { get; set; } = 90;
    public string? Code { get; set; }
    public DateOnly? ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public DayOfWeek[] Weekdays { get; set; } = [];
}
