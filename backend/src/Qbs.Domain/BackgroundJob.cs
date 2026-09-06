namespace Qbs.Domain;

public sealed class BackgroundJob : Entity
{
    public string Kind { get; set; } = "";
    public Guid ResourceId { get; set; }
    public long ExpectedRevision { get; set; }
    public int Attempt { get; set; }
    public string State { get; set; } = "Queued";
    public DateTimeOffset AvailableAt { get; set; }
    public DateTimeOffset? LeaseUntil { get; set; }
    public bool Relayed { get; set; }
    public string? Error { get; set; }
    public string Payload { get; set; } = "";
    public string? Result { get; set; }
}
