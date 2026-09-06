using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Clients;

public sealed class GetPrintRequestHandler(AdminCatalog catalog) : IRequestHandler<GetPrintRequest, PrintRequest>
{
    public async Task<PrintRequest> Handle(GetPrintRequest request, CancellationToken ct) =>
        await catalog.Get<PrintRequest>(request.Id) ?? throw new StudioException(404, "Request not found.");
}
