namespace Qbs.Domain;

public sealed record AppliedDiscount(
    Guid? RuleId,
    string? Kind,
    decimal Percentage,
    Money Amount,
    string? CodeError
);
