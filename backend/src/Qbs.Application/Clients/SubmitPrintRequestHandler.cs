using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Clients;

public sealed class SubmitPrintRequestHandler(ClientWorkflows clients) : IRequestHandler<SubmitPrintRequest, PrintRequest>
{
    public Task<PrintRequest> Handle(SubmitPrintRequest request, CancellationToken ct) =>
        clients.Submit(request.Client, request.Value);
}
