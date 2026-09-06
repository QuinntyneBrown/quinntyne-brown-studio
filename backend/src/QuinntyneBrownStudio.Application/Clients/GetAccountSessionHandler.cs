using MediatR;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Application.Ports;
namespace QuinntyneBrownStudio.Application.Clients;
public sealed class GetAccountSessionHandler(IAccountContext context) : IRequestHandler<GetAccountSession, AccountSession> { public Task<AccountSession> Handle(GetAccountSession request, CancellationToken ct) => Task.FromResult(context.Session()); }
