using MediatR;
using QuinntyneBrownStudio.Domain.Entities;

namespace QuinntyneBrownStudio.Application.Inquiries;

public sealed record ReviewInquiry(Guid Id, Guid Administrator, long ExpectedVersion) : IRequest<Inquiry>;
