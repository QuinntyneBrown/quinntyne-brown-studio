using MediatR;
using DomainEntities = Qbs.Domain.Entities;

namespace Qbs.Application.Catalog.DiscountConfiguration;

public sealed class SaveDiscountConfigurationHandler(AdminCatalog catalog)
    : IRequestHandler<SaveDiscountConfiguration, DomainEntities.DiscountConfiguration>
{
    public Task<DomainEntities.DiscountConfiguration> Handle(
        SaveDiscountConfiguration request,
        CancellationToken ct
    ) => catalog.Save(request.Value, request.Id);
}
