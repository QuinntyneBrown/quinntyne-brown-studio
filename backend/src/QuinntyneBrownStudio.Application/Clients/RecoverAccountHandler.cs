using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Clients;

public sealed class RecoverAccountHandler(IIdentityAccounts accounts) : IRequestHandler<RecoverAccount, object>
{
    public async Task<object> Handle(RecoverAccount request, CancellationToken ct) {
        await accounts.Recover(request.Email);
        return new { message = "If the account is eligible, recovery instructions will be sent." };
    }
}
