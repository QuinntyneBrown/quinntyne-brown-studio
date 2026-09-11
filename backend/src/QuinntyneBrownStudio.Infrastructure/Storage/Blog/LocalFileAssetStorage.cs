using QuinntyneBrownStudio.Application.Ports.Blog;
using QuinntyneBrownStudio.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace QuinntyneBrownStudio.Infrastructure.Storage.Blog;

public class LocalFileAssetStorage(Microsoft.Extensions.Options.IOptions<BlogStorageOptions> options) : IAssetStorage
{
    private string StoragePath => options.Value.Path;
    private string BaseUrl => "/blog/assets";

    private string SafePath(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name != Path.GetFileName(name) || name.Contains('\\') || name.Contains('/') || name.Contains(".."))
            throw new ArgumentException("Invalid asset file name.", nameof(name));
        return Path.Combine(StoragePath, name);
    }

    public async Task SaveAsync(string storedFileName, Stream stream, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(StoragePath);
        var filePath = SafePath(storedFileName);
        await using var fileStream = File.Create(filePath);
        await stream.CopyToAsync(fileStream, cancellationToken);
    }

    public Task<Stream?> GetAsync(string storedFileName, CancellationToken cancellationToken = default)
    {
        var filePath = SafePath(storedFileName);
        if (!File.Exists(filePath) || new FileInfo(filePath).Length == 0)
            return Task.FromResult<Stream?>(null);
        Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read,
            bufferSize: 4096, FileOptions.SequentialScan | FileOptions.Asynchronous);
        return Task.FromResult<Stream?>(stream);
    }

    public string GetFilePath(string storedFileName) => SafePath(storedFileName);

    public Task DeleteAsync(string storedFileName, CancellationToken cancellationToken = default)
    {
        var filePath = SafePath(storedFileName);
        if (File.Exists(filePath)) File.Delete(filePath);
        return Task.CompletedTask;
    }

    public string GetUrl(string storedFileName) => $"{BaseUrl}/{storedFileName}";

    public bool Exists(string storedFileName) => File.Exists(SafePath(storedFileName));
}
