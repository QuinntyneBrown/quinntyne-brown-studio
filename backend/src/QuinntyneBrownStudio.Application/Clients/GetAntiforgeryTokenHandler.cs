using MediatR;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Application.Ports;
namespace QuinntyneBrownStudio.Application.Clients;
public sealed class GetAntiforgeryTokenHandler(IAccountContext context) : IRequestHandler<GetAntiforgeryToken, AntiforgeryToken> { public Task<AntiforgeryToken> Handle(GetAntiforgeryToken request, CancellationToken ct) => Task.FromResult(context.Antiforgery()); }
