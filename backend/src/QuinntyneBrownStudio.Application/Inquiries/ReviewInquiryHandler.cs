using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Inquiries;

/// <summary>Records who reviewed an inquiry and when; the submitted message never changes.</summary>
public sealed class ReviewInquiryHandler(IStudioStore store, IClock clock) : IRequestHandler<ReviewInquiry, Inquiry>
{
    public Task<Inquiry> Handle(ReviewInquiry request, CancellationToken ct) =>
        store.Run(
            nameof(Inquiry),
            async tx =>
            {
                var inquiry =
                    await tx.Get<Inquiry>(request.Id) ?? throw new StudioException(404, "Inquiry not found.");
                inquiry.State = "Reviewed";
                inquiry.ReviewedBy = request.Administrator;
                inquiry.ReviewedAt = clock.UtcNow;
                await tx.Save(inquiry, request.ExpectedVersion);
                return inquiry;
            },
            ct
        );
}
