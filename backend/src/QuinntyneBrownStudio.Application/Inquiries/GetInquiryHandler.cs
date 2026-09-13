using MediatR;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Inquiries;

public sealed class GetInquiryHandler(AdminCatalog catalog) : IRequestHandler<GetInquiry, Inquiry>
{
    public async Task<Inquiry> Handle(GetInquiry request, CancellationToken ct) =>
        await catalog.Get<Inquiry>(request.Id) ?? throw new StudioException(404, "Inquiry not found.");
}
