using MediatR;
using Qbs.Domain.Models;

namespace Qbs.Application.Clients;

public sealed class PreviewPrintRequestHandler(ClientWorkflows clients) : IRequestHandler<PreviewPrintRequest, PrintPreview>
{
    public Task<PrintPreview> Handle(PreviewPrintRequest request, CancellationToken ct) =>
        clients.Preview(request.Client, request.Value);
}
