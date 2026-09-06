namespace Qbs.Domain.Models;

public sealed class PrintLine
{
    public Guid PhotoId { get; set; }
    public Guid OptionId { get; set; }
    public long OptionRevision { get; set; }
    public int Quantity { get; set; }
    public string Name { get; set; } = "";
    public string Dimensions { get; set; } = "";
    public string Finish { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }
}
