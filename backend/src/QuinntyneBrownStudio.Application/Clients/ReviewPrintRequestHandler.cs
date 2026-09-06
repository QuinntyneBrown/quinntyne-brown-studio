using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Clients;

public sealed class ReviewPrintRequestHandler(ClientWorkflows clients) : IRequestHandler<ReviewPrintRequest, PrintRequest>
{
    public Task<PrintRequest> Handle(ReviewPrintRequest request, CancellationToken ct) =>
        clients.Review(request.Id, request.Administrator, request.ExpectedVersion);
}
