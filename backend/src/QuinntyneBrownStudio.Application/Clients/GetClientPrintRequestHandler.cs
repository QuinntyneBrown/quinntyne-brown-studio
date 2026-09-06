using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Clients;

public sealed class GetClientPrintRequestHandler(ClientWorkflows clients) : IRequestHandler<GetClientPrintRequest, PrintRequest>
{
    public Task<PrintRequest> Handle(GetClientPrintRequest request, CancellationToken ct) =>
        clients.PrintRequest(request.Client, request.Id);
}
