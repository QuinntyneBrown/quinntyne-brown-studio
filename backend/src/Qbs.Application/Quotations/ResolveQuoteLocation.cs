using MediatR;
using Qbs.Domain.ValueObjects;

namespace Qbs.Application.Quotations;

public sealed record ResolveQuoteLocation(string Address) : IRequest<ResolvedLocation[]>;
