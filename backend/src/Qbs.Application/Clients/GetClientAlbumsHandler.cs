using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Clients;

public sealed class GetClientAlbumsHandler(ClientWorkflows clients) : IRequestHandler<GetClientAlbums, object>
{
    public Task<object> Handle(GetClientAlbums request, CancellationToken ct) =>
        clients.Albums(request.Client, request.Id);
}
