using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Catalog.RateConfiguration;

public sealed class GetRateConfigurationHandler(AdminCatalog catalog) : IRequestHandler<GetRateConfiguration, Qbs.Domain.Entities.RateConfiguration>
{
    public async Task<Qbs.Domain.Entities.RateConfiguration> Handle(GetRateConfiguration request, CancellationToken ct) =>
        await catalog.Get<Qbs.Domain.Entities.RateConfiguration>(AdminCatalog.ConfigurationId) ?? new Qbs.Domain.Entities.RateConfiguration { Id = AdminCatalog.ConfigurationId };
}
