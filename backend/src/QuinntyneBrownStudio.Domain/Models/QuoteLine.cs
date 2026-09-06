using QuinntyneBrownStudio.Domain.ValueObjects;

namespace QuinntyneBrownStudio.Domain.Models;

public sealed record QuoteLine(
    string Kind,
    int? LocationIndex,
    decimal Quantity,
    Money UnitPrice,
    Money Amount
);
