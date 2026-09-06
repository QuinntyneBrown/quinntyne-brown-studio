namespace Qbs.Domain;

public sealed record QuoteLine(
    string Kind,
    int? LocationIndex,
    decimal Quantity,
    Money UnitPrice,
    Money Amount
);
