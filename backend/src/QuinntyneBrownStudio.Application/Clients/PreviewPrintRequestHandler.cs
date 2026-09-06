using MediatR;
using QuinntyneBrownStudio.Domain.Models;

namespace QuinntyneBrownStudio.Application.Clients;

public sealed class PreviewPrintRequestHandler(ClientWorkflows clients) : IRequestHandler<PreviewPrintRequest, PrintPreview>
{
    public Task<PrintPreview> Handle(PreviewPrintRequest request, CancellationToken ct) =>
        clients.Preview(request.Client, request.Value);
}
