namespace Qbs.Domain;

public sealed record Money(decimal Amount, string Currency = "CAD");
