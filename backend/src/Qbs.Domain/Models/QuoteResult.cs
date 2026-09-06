using Qbs.Domain.Entities;
using Qbs.Domain.ValueObjects;

namespace Qbs.Domain.Models;

public sealed record QuoteResult(
    long InputRevision,
    long ConfigurationRevision,
    QuoteLine[] Lines,
    Money Subtotal,
    AppliedDiscount Discount,
    Money Total,
    AvailabilityResult Availability
);
