using Qbs.Domain.ValueObjects;

namespace Qbs.Domain.Models;

public sealed record QuoteStudioOption(Guid Id, string Name, ResolvedLocation ResolvedAddress, decimal HourlyFee);
