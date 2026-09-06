using MediatR;
using QuinntyneBrownStudio.Application.Ports;
namespace QuinntyneBrownStudio.Application.Diagnostics;
public sealed class GetDevelopmentMailHandler(IDevelopmentDiagnostics diagnostics) : IRequestHandler<GetDevelopmentMail, object> { public Task<object> Handle(GetDevelopmentMail request, CancellationToken ct) => Task.FromResult(diagnostics.Messages()); }
