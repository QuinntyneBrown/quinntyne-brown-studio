using System.Net;

namespace QuinntyneBrownStudio.AcceptanceTests;

public sealed class StalledTokenContent : HttpContent
{
    protected override bool TryComputeLength(out long length) { length = 0; return false; }
    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) => SerializeToStreamAsync(stream, context, CancellationToken.None);
    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken)
        => await Task.Delay(Timeout.Infinite, cancellationToken);
}
