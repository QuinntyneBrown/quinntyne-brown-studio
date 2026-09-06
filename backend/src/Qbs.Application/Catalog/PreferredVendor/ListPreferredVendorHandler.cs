using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Catalog.PreferredVendor;

public sealed class ListPreferredVendorHandler(AdminCatalog catalog) : IRequestHandler<ListPreferredVendor, Qbs.Domain.Entities.PreferredVendor[]>
{
    public Task<Qbs.Domain.Entities.PreferredVendor[]> Handle(ListPreferredVendor request, CancellationToken ct) =>
        catalog.List<Qbs.Domain.Entities.PreferredVendor>();
}
