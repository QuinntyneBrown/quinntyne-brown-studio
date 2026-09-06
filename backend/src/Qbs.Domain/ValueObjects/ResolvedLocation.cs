namespace Qbs.Domain.ValueObjects;

public sealed class ResolvedLocation
{
    public string Label { get; set; } = "";
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
}
