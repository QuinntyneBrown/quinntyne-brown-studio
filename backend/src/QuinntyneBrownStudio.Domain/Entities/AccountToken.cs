namespace QuinntyneBrownStudio.Domain.Entities;

public sealed class AccountToken : Entity
{
    public Guid AccountId { get; set; }
    public string Digest { get; set; } = "";
    public string Purpose { get; set; } = "";
    public DateTimeOffset ExpiresAt { get; set; }
    public bool Used { get; set; }
}
