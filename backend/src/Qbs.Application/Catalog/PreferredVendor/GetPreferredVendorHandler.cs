using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Catalog.PreferredVendor;

public sealed class GetPreferredVendorHandler(AdminCatalog catalog) : IRequestHandler<GetPreferredVendor, Qbs.Domain.Entities.PreferredVendor>
{
    public async Task<Qbs.Domain.Entities.PreferredVendor> Handle(GetPreferredVendor request, CancellationToken ct) =>
        await catalog.Get<Qbs.Domain.Entities.PreferredVendor>(request.Id) ?? throw new StudioException(404, "Record not found.");
}
