using MediatR;
using QuinntyneBrownStudio.Domain.ValueObjects;

namespace QuinntyneBrownStudio.Application.Quotations;

public sealed record ResolveQuoteLocation(string Address) : IRequest<ResolvedLocation[]>;
