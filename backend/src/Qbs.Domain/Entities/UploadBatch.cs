using Qbs.Domain.Models;

namespace Qbs.Domain.Entities;

public sealed class UploadBatch : Entity
{
    public Guid SessionId { get; set; }
    public UploadEntry[] Files { get; set; } = [];
}
