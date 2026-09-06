using QuinntyneBrownStudio.Domain.Models;

namespace QuinntyneBrownStudio.Domain.Entities;

public sealed class UploadBatch : Entity
{
    public Guid SessionId { get; set; }
    public UploadEntry[] Files { get; set; } = [];
}
