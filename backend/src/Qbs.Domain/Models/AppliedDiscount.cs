using Qbs.Domain.ValueObjects;

namespace Qbs.Domain.Models;

public sealed record AppliedDiscount(
    Guid? RuleId,
    string? Kind,
    decimal Percentage,
    Money Amount,
    string? CodeError
);
