namespace Qbs.Domain;

public sealed record QuoteResult(
    long InputRevision,
    long ConfigurationRevision,
    QuoteLine[] Lines,
    Money Subtotal,
    AppliedDiscount Discount,
    Money Total,
    AvailabilityResult Availability
);
