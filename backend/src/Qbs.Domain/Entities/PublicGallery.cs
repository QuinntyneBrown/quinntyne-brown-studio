namespace Qbs.Domain.Entities;

public sealed class PublicGallery : Entity
{
    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    public Guid[] PhotoIds { get; set; } = [];
    public bool Published { get; set; }
}
