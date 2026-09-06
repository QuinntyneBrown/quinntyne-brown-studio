using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Catalog.RateConfiguration;

public sealed class GetRateConfigurationHandler(AdminCatalog catalog) : IRequestHandler<GetRateConfiguration, QuinntyneBrownStudio.Domain.Entities.RateConfiguration>
{
    public async Task<QuinntyneBrownStudio.Domain.Entities.RateConfiguration> Handle(GetRateConfiguration request, CancellationToken ct) =>
        await catalog.Get<QuinntyneBrownStudio.Domain.Entities.RateConfiguration>(AdminCatalog.ConfigurationId) ?? new QuinntyneBrownStudio.Domain.Entities.RateConfiguration { Id = AdminCatalog.ConfigurationId };
}
