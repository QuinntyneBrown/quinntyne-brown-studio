using MediatR;
using Qbs.Domain;

namespace Qbs.Application;

public sealed class SavePreferredVendorHandler(AdminCatalog catalog)
    : IRequestHandler<SavePreferredVendor, PreferredVendor>
{
    public Task<PreferredVendor> Handle(SavePreferredVendor request, CancellationToken ct) =>
        catalog.Save(request.Value, request.Id);
}
