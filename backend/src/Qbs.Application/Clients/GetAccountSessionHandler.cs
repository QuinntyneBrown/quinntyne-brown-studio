using MediatR;
using Qbs.Domain.Models;
using Qbs.Application.Ports;
namespace Qbs.Application.Clients;
public sealed class GetAccountSessionHandler(IAccountContext context) : IRequestHandler<GetAccountSession, AccountSession> { public Task<AccountSession> Handle(GetAccountSession request, CancellationToken ct) => Task.FromResult(context.Session()); }
