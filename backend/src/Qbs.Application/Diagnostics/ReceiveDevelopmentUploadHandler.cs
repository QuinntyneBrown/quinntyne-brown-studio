using MediatR;
using Qbs.Application.Ports;
namespace Qbs.Application.Diagnostics;
public sealed class ReceiveDevelopmentUploadHandler(IDevelopmentDiagnostics diagnostics) : IRequestHandler<ReceiveDevelopmentUpload> { public Task Handle(ReceiveDevelopmentUpload request, CancellationToken ct) => diagnostics.Receive(request.Key, request.Operation, request.BlockId, request.Body, ct); }
