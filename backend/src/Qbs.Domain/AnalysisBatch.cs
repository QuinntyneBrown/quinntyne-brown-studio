namespace Qbs.Domain;

public sealed class AnalysisBatch : Entity
{
    public Guid SessionId { get; set; }
    public Guid[] JobIds { get; set; } = [];
}
