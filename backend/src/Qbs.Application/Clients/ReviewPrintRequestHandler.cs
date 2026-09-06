using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Clients;

public sealed class ReviewPrintRequestHandler(ClientWorkflows clients) : IRequestHandler<ReviewPrintRequest, PrintRequest>
{
    public Task<PrintRequest> Handle(ReviewPrintRequest request, CancellationToken ct) =>
        clients.Review(request.Id, request.Administrator, request.ExpectedVersion);
}
