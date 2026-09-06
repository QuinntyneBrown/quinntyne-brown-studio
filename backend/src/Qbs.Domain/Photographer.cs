namespace Qbs.Domain;

public sealed class Photographer : Entity
{
    public string Name { get; set; } = "";
    public bool Active { get; set; } = true;
}
