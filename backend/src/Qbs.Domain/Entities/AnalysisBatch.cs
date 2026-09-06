namespace Qbs.Domain.Entities;

public sealed class AnalysisBatch : Entity
{
    public Guid SessionId { get; set; }
    public Guid[] JobIds { get; set; } = [];
}
