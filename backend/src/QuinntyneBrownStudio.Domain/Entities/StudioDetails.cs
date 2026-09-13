namespace QuinntyneBrownStudio.Domain.Entities;

/// <summary>
/// The studio's public contact details, edited by administrators and read by the contact page
/// and inquiry notifications. Every field is optional: an unconfigured detail is absent, never invented.
/// </summary>
public sealed class StudioDetails : Entity
{
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Hours { get; set; }
    public string? ReplyNote { get; set; }
}
