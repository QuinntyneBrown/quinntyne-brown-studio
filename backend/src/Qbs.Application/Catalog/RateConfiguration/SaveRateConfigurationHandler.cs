using MediatR;
using DomainEntities = Qbs.Domain.Entities;

namespace Qbs.Application.Catalog.RateConfiguration;

public sealed class SaveRateConfigurationHandler(AdminCatalog catalog)
    : IRequestHandler<SaveRateConfiguration, DomainEntities.RateConfiguration>
{
    public Task<DomainEntities.RateConfiguration> Handle(SaveRateConfiguration request, CancellationToken ct) =>
        catalog.Save(request.Value, request.Id);
}
