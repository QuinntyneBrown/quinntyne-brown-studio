using MediatR;
namespace Qbs.Application.Diagnostics;
public sealed class GetHealthHandler : IRequestHandler<GetHealth, object> { public Task<object> Handle(GetHealth request, CancellationToken ct) => Task.FromResult<object>(new { status = "Ready" }); }
