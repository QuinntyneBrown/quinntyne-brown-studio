namespace Qbs.Domain;

public sealed class Album : Entity
{
    public Guid ClientId { get; set; }
    public string Name { get; set; } = "";
    public Guid[] PhotoIds { get; set; } = [];
    public Guid[]? OrderedPhotoIds { get; set; }
}
