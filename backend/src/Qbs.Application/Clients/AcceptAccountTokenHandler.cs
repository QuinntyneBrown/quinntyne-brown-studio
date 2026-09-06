using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Clients;

public sealed class AcceptAccountTokenHandler(IIdentityAccounts accounts) : IRequestHandler<AcceptAccountToken, object>
{
    public Task<object> Handle(AcceptAccountToken request, CancellationToken ct) =>
        accounts.Accept(request.Token, request.Password, request.Purpose);
}
