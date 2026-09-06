namespace Qbs.Domain;

public sealed class Promotion : Entity
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal IndicativePrice { get; set; }
    public bool Published { get; set; }
}
