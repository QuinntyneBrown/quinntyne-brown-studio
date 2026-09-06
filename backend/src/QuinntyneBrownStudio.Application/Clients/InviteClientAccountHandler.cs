using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Clients;

public sealed class InviteClientAccountHandler(IIdentityAccounts accounts) : IRequestHandler<InviteClientAccount, object>
{
    public async Task<object> Handle(InviteClientAccount request, CancellationToken ct) {
        return new { invitationId = await accounts.Invite(request.Email), status = "Queued" };
    }
}
