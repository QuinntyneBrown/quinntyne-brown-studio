using MediatR;
using Qbs.Domain.Models;

namespace Qbs.Application.Quotations;

public sealed record GetQuoteStudios : IRequest<QuoteStudioOption[]>;
