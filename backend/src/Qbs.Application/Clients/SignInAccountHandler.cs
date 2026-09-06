using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Clients;

public sealed class SignInAccountHandler(IIdentityAccounts accounts) : IRequestHandler<SignInAccount, object>
{
    public Task<object> Handle(SignInAccount request, CancellationToken ct) =>
        accounts.Login(request.Email, request.Password);
}
