namespace QuinntyneBrownStudio.Application.Ports;

public interface IRawPreviewConverter
{
    Task<Stream> Convert(Stream original, string name, CancellationToken ct);
}
