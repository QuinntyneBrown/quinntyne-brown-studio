using MediatR;
using QuinntyneBrownStudio.Domain.Entities;

namespace QuinntyneBrownStudio.Application.Inquiries;

/// <summary>The contact form as the visitor sent it; validated by <see cref="SubmitInquiryValidator"/>.</summary>
public sealed record SubmitInquiry(
    string Name,
    string Email,
    string? Phone,
    string Interest,
    string Message,
    bool Consent
) : IRequest<Inquiry>;
