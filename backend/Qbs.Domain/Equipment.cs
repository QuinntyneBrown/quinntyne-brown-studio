namespace Qbs.Domain;

public sealed class Equipment : Entity
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public int Quantity { get; set; }
    public decimal? ReferenceRentalRate { get; set; }
}
