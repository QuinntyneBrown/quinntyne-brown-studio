using MediatR;
using Qbs.Domain.Models;
using Qbs.Application.Ports;
namespace Qbs.Application.Clients;
public sealed class GetAntiforgeryTokenHandler(IAccountContext context) : IRequestHandler<GetAntiforgeryToken, AntiforgeryToken> { public Task<AntiforgeryToken> Handle(GetAntiforgeryToken request, CancellationToken ct) => Task.FromResult(context.Antiforgery()); }
