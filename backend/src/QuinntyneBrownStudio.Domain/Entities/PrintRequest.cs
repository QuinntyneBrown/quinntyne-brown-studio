using QuinntyneBrownStudio.Domain.Models;

namespace QuinntyneBrownStudio.Domain.Entities;

public sealed class PrintRequest : Entity
{
    public Guid ClientId { get; set; }
    public string IdempotencyKey { get; set; } = "";
    public string PayloadHash { get; set; } = "";
    public PrintLine[] Lines { get; set; } = [];
    public string? Notes { get; set; }
    public decimal Total { get; set; }
    public string State { get; set; } = "Submitted";
    public Guid? ReviewedBy { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
}
