namespace Qbs.Domain;

public sealed class RateConfiguration : Entity
{
    public Dictionary<ServiceKind, decimal?> ServiceRates { get; set; } = [];
    public Dictionary<string, decimal?> CostRates { get; set; } = [];
}
