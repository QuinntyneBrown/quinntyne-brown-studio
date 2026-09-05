namespace Qbs.Domain;

public abstract class Entity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public long Version { get; set; }
    public long ExpectedVersion { get; set; }
}
