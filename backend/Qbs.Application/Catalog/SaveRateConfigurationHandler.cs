using MediatR;
using Qbs.Domain;

namespace Qbs.Application;

public sealed class SaveRateConfigurationHandler(AdminCatalog catalog)
    : IRequestHandler<SaveRateConfiguration, RateConfiguration>
{
    public Task<RateConfiguration> Handle(SaveRateConfiguration request, CancellationToken ct) =>
        catalog.Save(request.Value, request.Id);
}
