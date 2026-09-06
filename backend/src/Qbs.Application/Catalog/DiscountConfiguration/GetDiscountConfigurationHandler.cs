using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Catalog.DiscountConfiguration;

public sealed class GetDiscountConfigurationHandler(AdminCatalog catalog) : IRequestHandler<GetDiscountConfiguration, Qbs.Domain.Entities.DiscountConfiguration>
{
    public async Task<Qbs.Domain.Entities.DiscountConfiguration> Handle(GetDiscountConfiguration request, CancellationToken ct) =>
        await catalog.Get<Qbs.Domain.Entities.DiscountConfiguration>(AdminCatalog.ConfigurationId) ?? new Qbs.Domain.Entities.DiscountConfiguration { Id = AdminCatalog.ConfigurationId };
}
