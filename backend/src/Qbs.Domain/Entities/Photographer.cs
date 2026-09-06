namespace Qbs.Domain.Entities;

public sealed class Photographer : Entity
{
    public string Name { get; set; } = "";
    public bool Active { get; set; } = true;
}
