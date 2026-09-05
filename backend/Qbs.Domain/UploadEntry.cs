namespace Qbs.Domain;

public sealed class UploadEntry
{
    public string ClientFileId { get; set; } = "";
    public string Name { get; set; } = "";
    public long Size { get; set; }
    public string Sha256 { get; set; } = "";
    public Guid? PhotoId { get; set; }
    public string? Rejection { get; set; }
}
