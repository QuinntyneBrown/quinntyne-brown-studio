using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Clients;

public sealed class InviteClientAccountHandler(IIdentityAccounts accounts) : IRequestHandler<InviteClientAccount, object>
{
    public async Task<object> Handle(InviteClientAccount request, CancellationToken ct) {
        return new { invitationId = await accounts.Invite(request.Email), status = "Queued" };
    }
}
