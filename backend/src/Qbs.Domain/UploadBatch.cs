namespace Qbs.Domain;

public sealed class UploadBatch : Entity
{
    public Guid SessionId { get; set; }
    public UploadEntry[] Files { get; set; } = [];
}
