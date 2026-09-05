using Qbs.Domain;

namespace Qbs.Application;

public interface IRawPreviewConverter
{
    Task<Stream> Convert(Stream original, string name, CancellationToken ct);
}
