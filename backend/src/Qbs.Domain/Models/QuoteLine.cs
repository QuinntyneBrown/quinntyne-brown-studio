using Qbs.Domain.ValueObjects;

namespace Qbs.Domain.Models;

public sealed record QuoteLine(
    string Kind,
    int? LocationIndex,
    decimal Quantity,
    Money UnitPrice,
    Money Amount
);
