using Qbs.Domain.Entities;
using Qbs.Domain.Models;

namespace Qbs.Application.Ports;

public interface IPhotoStorage
{
    Task<StorageGrant> Grant(string key, CancellationToken ct);
    Task Finalize(SessionPhoto photo, CancellationToken ct);
    Task<Stream> Read(string key, CancellationToken ct);
    Task Write(string key, Stream content, CancellationToken ct);
    Task Delete(string key, CancellationToken ct);
}
