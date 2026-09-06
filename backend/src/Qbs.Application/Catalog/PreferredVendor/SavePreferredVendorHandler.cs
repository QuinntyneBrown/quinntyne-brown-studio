using MediatR;
using DomainEntities = Qbs.Domain.Entities;

namespace Qbs.Application.Catalog.PreferredVendor;

public sealed class SavePreferredVendorHandler(AdminCatalog catalog)
    : IRequestHandler<SavePreferredVendor, DomainEntities.PreferredVendor>
{
    public Task<DomainEntities.PreferredVendor> Handle(SavePreferredVendor request, CancellationToken ct) =>
        catalog.Save(request.Value, request.Id);
}
