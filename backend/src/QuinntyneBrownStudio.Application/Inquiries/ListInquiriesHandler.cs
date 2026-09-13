using MediatR;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;

namespace QuinntyneBrownStudio.Application.Inquiries;

public sealed class ListInquiriesHandler(AdminCatalog catalog) : IRequestHandler<ListInquiries, Inquiry[]>
{
    public async Task<Inquiry[]> Handle(ListInquiries request, CancellationToken ct) =>
        (await catalog.List<Inquiry>())
            .Where(x => request.State == null || x.State == request.State)
            .OrderByDescending(x => x.SubmittedAt)
            .ToArray();
}
