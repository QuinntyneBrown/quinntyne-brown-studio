using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Clients;

public sealed class SaveClientAlbumHandler(ClientWorkflows clients) : IRequestHandler<SaveClientAlbum, Album>
{
    public Task<Album> Handle(SaveClientAlbum request, CancellationToken ct) =>
        clients.SaveAlbum(request.Client, request.Id, request.Value);
}
