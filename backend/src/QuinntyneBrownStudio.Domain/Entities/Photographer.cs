namespace QuinntyneBrownStudio.Domain.Entities;

public sealed class Photographer : Entity
{
    public string Name { get; set; } = "";
    public bool Active { get; set; } = true;

    /// <summary>Set once when the record is first saved; orders the studio's team.</summary>
    public DateTimeOffset CreatedAt { get; set; }
}
