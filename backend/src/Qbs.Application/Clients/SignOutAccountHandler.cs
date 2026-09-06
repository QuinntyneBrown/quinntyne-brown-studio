using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Clients;

public sealed class SignOutAccountHandler(IIdentityAccounts accounts) : IRequestHandler<SignOutAccount, object>
{
    public async Task<object> Handle(SignOutAccount request, CancellationToken ct) {
        await accounts.Logout();
        return new { authenticated = false };
    }
}
