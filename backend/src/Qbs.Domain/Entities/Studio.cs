using Qbs.Domain.ValueObjects;

namespace Qbs.Domain.Entities;

public sealed class Studio : Entity
{
    public string Name { get; set; } = "";
    public ResolvedLocation ResolvedAddress { get; set; } = new();
    public decimal HourlyFee { get; set; }
    public bool Enabled { get; set; }
    public bool IsBase { get; set; }
}
