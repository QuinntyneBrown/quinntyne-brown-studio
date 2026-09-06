using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Clients;

public sealed class GetClientPrintRequestHandler(ClientWorkflows clients) : IRequestHandler<GetClientPrintRequest, PrintRequest>
{
    public Task<PrintRequest> Handle(GetClientPrintRequest request, CancellationToken ct) =>
        clients.PrintRequest(request.Client, request.Id);
}
