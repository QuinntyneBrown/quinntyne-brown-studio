using MediatR;
using QuinntyneBrownStudio.Domain.Entities;

namespace QuinntyneBrownStudio.Application.Inquiries;

public sealed record GetInquiry(Guid Id) : IRequest<Inquiry>;
