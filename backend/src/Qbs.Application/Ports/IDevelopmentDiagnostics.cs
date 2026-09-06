namespace Qbs.Application.Ports;
public interface IDevelopmentDiagnostics {
  Task Receive(string key, string operation, string? blockId, Stream body, CancellationToken ct);
  object Messages();
}
