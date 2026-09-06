using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Clients;

public sealed class RecoverAccountHandler(IIdentityAccounts accounts) : IRequestHandler<RecoverAccount, object>
{
    public async Task<object> Handle(RecoverAccount request, CancellationToken ct) {
        await accounts.Recover(request.Email);
        return new { message = "If the account is eligible, recovery instructions will be sent." };
    }
}
