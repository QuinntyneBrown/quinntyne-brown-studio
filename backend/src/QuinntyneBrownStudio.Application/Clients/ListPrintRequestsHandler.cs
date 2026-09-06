using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Clients;

public sealed class ListPrintRequestsHandler(AdminCatalog catalog) : IRequestHandler<ListPrintRequests, PrintRequest[]>
{
    public async Task<PrintRequest[]> Handle(ListPrintRequests request, CancellationToken ct) =>
        (await catalog.List<PrintRequest>()).Where(x => request.State == null || x.State == request.State).ToArray();
}
