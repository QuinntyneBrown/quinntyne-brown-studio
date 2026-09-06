namespace QuinntyneBrownStudio.Domain.ValueObjects;

public sealed record Money(decimal Amount, string Currency = "CAD");
