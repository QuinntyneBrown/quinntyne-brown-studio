using QuinntyneBrownStudio.Domain.Enums;

namespace QuinntyneBrownStudio.Domain.Entities;

public sealed class SessionPhoto : Entity
{
    public Guid SessionId { get; set; }
    public Guid BatchId { get; set; }
    public string Name { get; set; } = "";
    public string Sha256 { get; set; } = "";
    public long Size { get; set; }
    public string StagingKey { get; set; } = "";
    public string OriginalKey { get; set; } = "";
    public string? PreviewKey { get; set; }
    public string? ThumbnailKey { get; set; }
    public PhotoState State { get; set; }
    public DateTimeOffset UploadedAt { get; set; }
    public string? Failure { get; set; }
}
