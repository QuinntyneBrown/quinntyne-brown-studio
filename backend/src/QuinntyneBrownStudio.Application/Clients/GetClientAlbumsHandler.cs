using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Clients;

public sealed class GetClientAlbumsHandler(ClientWorkflows clients) : IRequestHandler<GetClientAlbums, object>
{
    public Task<object> Handle(GetClientAlbums request, CancellationToken ct) =>
        clients.Albums(request.Client, request.Id);
}
