using MediatR;
using Qbs.Application.Ports;
namespace Qbs.Application.Diagnostics;
public sealed class GetDevelopmentMailHandler(IDevelopmentDiagnostics diagnostics) : IRequestHandler<GetDevelopmentMail, object> { public Task<object> Handle(GetDevelopmentMail request, CancellationToken ct) => Task.FromResult(diagnostics.Messages()); }
