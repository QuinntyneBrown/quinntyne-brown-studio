using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Clients;

public sealed class ListPrintRequestsHandler(AdminCatalog catalog) : IRequestHandler<ListPrintRequests, PrintRequest[]>
{
    public async Task<PrintRequest[]> Handle(ListPrintRequests request, CancellationToken ct) =>
        (await catalog.List<PrintRequest>()).Where(x => request.State == null || x.State == request.State).ToArray();
}
