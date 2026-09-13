namespace QuinntyneBrownStudio.Api.Models;

/// <summary>The contact form fields as posted; <see cref="Website"/> is the hidden field only automated agents fill.</summary>
public sealed class InquiryForm
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Interest { get; set; }
    public string? Message { get; set; }
    public bool Consent { get; set; }
    public string? Website { get; set; }
}
