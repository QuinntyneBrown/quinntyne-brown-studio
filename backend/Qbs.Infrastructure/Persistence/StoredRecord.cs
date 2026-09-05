namespace Qbs.Infrastructure;

public sealed class StoredRecord
{
    public Guid Id { get; set; }
    public string Kind { get; set; } = "";
    public string? UniqueKey { get; set; }
    public string Payload { get; set; } = "";
    public long Version { get; set; }
}
