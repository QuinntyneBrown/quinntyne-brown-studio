using MediatR;
using Qbs.Domain;

namespace Qbs.Application;

public sealed record CalculateQuote(QuoteInput Input) : IRequest<QuoteResult>;
