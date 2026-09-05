using Qbs.Domain;

namespace Qbs.Application;

public interface IPhotoStorage
{
    Task<StorageGrant> Grant(string key, CancellationToken ct);
    Task Finalize(SessionPhoto photo, CancellationToken ct);
    Task<Stream> Read(string key, CancellationToken ct);
    Task Write(string key, Stream content, CancellationToken ct);
    Task Delete(string key, CancellationToken ct);
}
