using MediatR;
using QuinntyneBrownStudio.Domain.Models;

namespace QuinntyneBrownStudio.Application.Quotations;

public sealed record GetQuoteStudios : IRequest<QuoteStudioOption[]>;
