using QuinntyneBrownStudio.Domain.ValueObjects;

namespace QuinntyneBrownStudio.Domain.Models;

public sealed class QuoteLocation
{
    public ResolvedLocation Location { get; set; } = new();
    public decimal ParkingAmount { get; set; }
    public Guid? StudioId { get; set; }
    public decimal StudioHours { get; set; }
}
