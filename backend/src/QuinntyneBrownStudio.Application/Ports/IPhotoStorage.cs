using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;

namespace QuinntyneBrownStudio.Application.Ports;

public interface IPhotoStorage
{
    Task<StorageGrant> Grant(string key, CancellationToken ct);
    Task Finalize(SessionPhoto photo, CancellationToken ct);
    Task<Stream> Read(string key, CancellationToken ct);
    Task Write(string key, Stream content, CancellationToken ct);
    Task Delete(string key, CancellationToken ct);
}
