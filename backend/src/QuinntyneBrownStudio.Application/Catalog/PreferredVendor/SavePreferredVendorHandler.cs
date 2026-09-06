using MediatR;
using DomainEntities = QuinntyneBrownStudio.Domain.Entities;

namespace QuinntyneBrownStudio.Application.Catalog.PreferredVendor;

public sealed class SavePreferredVendorHandler(AdminCatalog catalog)
    : IRequestHandler<SavePreferredVendor, DomainEntities.PreferredVendor>
{
    public Task<DomainEntities.PreferredVendor> Handle(SavePreferredVendor request, CancellationToken ct) =>
        catalog.Save(request.Value, request.Id);
}
