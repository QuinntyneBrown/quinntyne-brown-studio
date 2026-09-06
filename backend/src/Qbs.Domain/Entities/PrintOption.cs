namespace Qbs.Domain.Entities;

public sealed class PrintOption : Entity
{
    public string Name { get; set; } = "";
    public string Dimensions { get; set; } = "";
    public string Finish { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public bool Enabled { get; set; }
}
