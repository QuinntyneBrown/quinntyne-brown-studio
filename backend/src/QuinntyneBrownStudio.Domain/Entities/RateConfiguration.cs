using QuinntyneBrownStudio.Domain.Enums;

namespace QuinntyneBrownStudio.Domain.Entities;

public sealed class RateConfiguration : Entity
{
    public Dictionary<ServiceKind, decimal?> ServiceRates { get; set; } = [];
    public Dictionary<string, decimal?> CostRates { get; set; } = [];
}
