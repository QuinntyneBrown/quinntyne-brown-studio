using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Clients;

public sealed class GetClientGalleriesHandler(ClientWorkflows clients) : IRequestHandler<GetClientGalleries, object>
{
    public Task<object> Handle(GetClientGalleries request, CancellationToken ct) =>
        clients.Galleries(request.Client, request.Id);
}
