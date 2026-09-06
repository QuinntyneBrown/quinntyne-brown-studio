using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Clients;

public sealed class AssignClientGalleryHandler(ClientWorkflows clients, IIdentityAccounts accounts) : IRequestHandler<AssignClientGallery, Session>
{
    public async Task<Session> Handle(AssignClientGallery request, CancellationToken ct) {
        await accounts.RequireClients(request.ClientIds);
        return await clients.Assign(request.Id, request.ClientIds, request.ExpectedVersion);
    }
}
