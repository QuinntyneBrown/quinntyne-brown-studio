using System.Xml.Linq;
using Microsoft.Extensions.Hosting;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Domain.Exceptions;
using QuinntyneBrownStudio.Infrastructure.Storage;
namespace QuinntyneBrownStudio.Infrastructure.Adapters;
public sealed class DevelopmentDiagnostics(IPhotoStorage storage, IEmailSender email, IHostEnvironment environment) : IDevelopmentDiagnostics {
  public async Task Receive(string key, string operation, string? blockId, Stream body, CancellationToken ct) {
    if ((!environment.IsDevelopment() && !environment.IsEnvironment("Testing")) || storage is not FilePhotoStorage files) throw new StudioException(404, "Not found.");
    if (operation == "block" && blockId != null) await files.Block(key, blockId, body, ct);
    else if (operation == "blocklist") {
      try {
        var xml = await XDocument.LoadAsync(body, LoadOptions.None, ct);
        await files.Commit(key, xml.Descendants("Latest").Select(element => element.Value).ToArray(), ct);
      } catch (System.Xml.XmlException) { throw new StudioException(400, "The block list is invalid."); }
    } else throw new StudioException(400, "The storage operation is invalid.");
  }
  public object Messages() {
    if (!environment.IsDevelopment() || email is not ControlledEmail controlled) throw new StudioException(404, "Not found.");
    return controlled.Messages.Select(message => new { id = message.Key, body = message.Value });
  }
}
