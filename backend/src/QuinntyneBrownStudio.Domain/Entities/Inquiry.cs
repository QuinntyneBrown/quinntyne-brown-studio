using QuinntyneBrownStudio.Domain.Enums;

namespace QuinntyneBrownStudio.Domain.Entities;

/// <summary>
/// A message sent from the contact page. An inquiry establishes no booking, quotation, or
/// reservation; administrators review it in the inbox.
/// </summary>
public sealed class Inquiry : Entity
{
    public string Reference { get; set; } = "";
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public ServiceKind Interest { get; set; }
    public string Message { get; set; } = "";
    public DateTimeOffset ConsentAt { get; set; }
    public DateTimeOffset SubmittedAt { get; set; }
    public string State { get; set; } = "Submitted";
    public Guid? ReviewedBy { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
}
