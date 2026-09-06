using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Catalog.DiscountConfiguration;

public sealed class GetDiscountConfigurationHandler(AdminCatalog catalog) : IRequestHandler<GetDiscountConfiguration, QuinntyneBrownStudio.Domain.Entities.DiscountConfiguration>
{
    public async Task<QuinntyneBrownStudio.Domain.Entities.DiscountConfiguration> Handle(GetDiscountConfiguration request, CancellationToken ct) =>
        await catalog.Get<QuinntyneBrownStudio.Domain.Entities.DiscountConfiguration>(AdminCatalog.ConfigurationId) ?? new QuinntyneBrownStudio.Domain.Entities.DiscountConfiguration { Id = AdminCatalog.ConfigurationId };
}
