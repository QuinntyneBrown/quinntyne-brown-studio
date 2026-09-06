using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.ValueObjects;

namespace QuinntyneBrownStudio.Domain.Models;

public sealed record QuoteResult(
    long InputRevision,
    long ConfigurationRevision,
    QuoteLine[] Lines,
    Money Subtotal,
    AppliedDiscount Discount,
    Money Total,
    AvailabilityResult Availability
);
