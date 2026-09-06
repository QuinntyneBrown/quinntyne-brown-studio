using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Clients;

public sealed class AssignClientGalleryHandler(ClientWorkflows clients, IIdentityAccounts accounts) : IRequestHandler<AssignClientGallery, Session>
{
    public async Task<Session> Handle(AssignClientGallery request, CancellationToken ct) {
        await accounts.RequireClients(request.ClientIds);
        return await clients.Assign(request.Id, request.ClientIds, request.ExpectedVersion);
    }
}
