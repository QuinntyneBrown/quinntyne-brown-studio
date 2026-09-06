using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Clients;

public sealed class ListClientAccountsHandler(IIdentityAccounts accounts) : IRequestHandler<ListClientAccounts, object>
{
    public Task<object> Handle(ListClientAccounts request, CancellationToken ct) =>
        accounts.Clients();
}
