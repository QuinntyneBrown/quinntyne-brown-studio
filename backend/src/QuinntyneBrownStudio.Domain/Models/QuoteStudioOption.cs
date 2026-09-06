using QuinntyneBrownStudio.Domain.ValueObjects;

namespace QuinntyneBrownStudio.Domain.Models;

public sealed record QuoteStudioOption(Guid Id, string Name, ResolvedLocation ResolvedAddress, decimal HourlyFee);
