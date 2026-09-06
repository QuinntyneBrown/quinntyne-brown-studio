using QuinntyneBrownStudio.Domain.ValueObjects;

namespace QuinntyneBrownStudio.Domain.Models;

public sealed record AppliedDiscount(
    Guid? RuleId,
    string? Kind,
    decimal Percentage,
    Money Amount,
    string? CodeError
);
