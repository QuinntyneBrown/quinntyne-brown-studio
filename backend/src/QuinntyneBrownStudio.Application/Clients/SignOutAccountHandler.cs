using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Clients;

public sealed class SignOutAccountHandler(IIdentityAccounts accounts) : IRequestHandler<SignOutAccount, object>
{
    public async Task<object> Handle(SignOutAccount request, CancellationToken ct) {
        await accounts.Logout();
        return new { authenticated = false };
    }
}
