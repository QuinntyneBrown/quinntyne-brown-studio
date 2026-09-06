using MediatR;
using Qbs.Domain;

namespace Qbs.Application;

public sealed class SaveDiscountConfigurationHandler(AdminCatalog catalog)
    : IRequestHandler<SaveDiscountConfiguration, DiscountConfiguration>
{
    public Task<DiscountConfiguration> Handle(
        SaveDiscountConfiguration request,
        CancellationToken ct
    ) => catalog.Save(request.Value, request.Id);
}
