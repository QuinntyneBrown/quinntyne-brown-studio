using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Clients;

public sealed class SaveClientAlbumHandler(ClientWorkflows clients) : IRequestHandler<SaveClientAlbum, Album>
{
    public Task<Album> Handle(SaveClientAlbum request, CancellationToken ct) =>
        clients.SaveAlbum(request.Client, request.Id, request.Value);
}
