using QuinntyneBrownStudio.Domain.Enums;

namespace QuinntyneBrownStudio.Domain.Models;

public sealed class QuoteInput
{
    public ServiceKind Service { get; set; }
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset EndsAt { get; set; }
    public QuoteLocation[] Locations { get; set; } = [];
    public int AssistantCount { get; set; }
    public int EquipmentUnits { get; set; }
    public int LunchCount { get; set; }
    public string? Code { get; set; }
    public Guid? PhotographerId { get; set; }
    public long InputRevision { get; set; }
}
