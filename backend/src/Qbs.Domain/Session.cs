namespace Qbs.Domain;

public sealed class Session : Entity
{
    public string Name { get; set; } = "";
    public ServiceKind Service { get; set; }
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset EndsAt { get; set; }
    public Guid? PhotographerId { get; set; }
    public Guid[] ClientIds { get; set; } = [];
    public int RetentionMonths { get; set; } = 12;
    public DateTimeOffset? ExpiresAt { get; set; }
    public long ExpiryRevision { get; set; }
    public long NoticeRevision { get; set; }
    public string RetentionState { get; set; } = "Active";
}
