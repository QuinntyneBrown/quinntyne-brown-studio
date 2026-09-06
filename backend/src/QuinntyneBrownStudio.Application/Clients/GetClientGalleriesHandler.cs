using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Clients;

public sealed class GetClientGalleriesHandler(ClientWorkflows clients) : IRequestHandler<GetClientGalleries, object>
{
    public Task<object> Handle(GetClientGalleries request, CancellationToken ct) =>
        clients.Galleries(request.Client, request.Id);
}
