using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Qbs.Application.Ports;
using Qbs.Domain.Entities;
using Qbs.Domain.Exceptions;
using Qbs.Domain.Models;
using Qbs.Domain.Policies;

namespace Qbs.Infrastructure.Storage;

public sealed class FilePhotoStorage(IConfiguration config) : IPhotoStorage
{
    private readonly string root = Path.GetFullPath(
        config["Development:PhotoDirectory"] ?? Path.Combine(Path.GetTempPath(), "qbs-photos")
    );

    public string Resolve(string key)
    {
        var path = Path.GetFullPath(
            Path.Combine(root, key.Replace('/', Path.DirectorySeparatorChar))
        );
        if (
            !path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
        )
            throw new StudioException(400, "Invalid storage key.");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        return path;
    }

    public Task<StorageGrant> Grant(string key, CancellationToken ct) =>
        Task.FromResult(
            new StorageGrant(
                "/api/admin/development-storage/" + key,
                DateTimeOffset.UtcNow.AddMinutes(15)
            )
        );

    public Task<Stream> Read(string key, CancellationToken ct)
    {
        var path = Resolve(key);
        if (!File.Exists(path))
            throw new StudioException(404, "Photo bytes are unavailable.");
        return Task.FromResult<Stream>(File.OpenRead(path));
    }

    public async Task Write(string key, Stream content, CancellationToken ct)
    {
        await using var file = File.Create(Resolve(key));
        await content.CopyToAsync(file, ct);
    }

    public Task Delete(string key, CancellationToken ct)
    {
        File.Delete(Resolve(key));
        return Task.CompletedTask;
    }

    public async Task Finalize(SessionPhoto photo, CancellationToken ct)
    {
        var original = Resolve(photo.OriginalKey);
        if (File.Exists(original))
        {
            await using var existing = File.OpenRead(original);
            await PhotoIntegrity.Validate(existing, photo, ct);
            return;
        }
        await using var stream = File.OpenRead(Resolve(photo.StagingKey));
        await PhotoIntegrity.Validate(stream, photo, ct);
        await using var target = new FileStream(
            original,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None
        );
        await stream.CopyToAsync(target, ct);
    }

    public async Task Block(string key, string block, Stream content, CancellationToken ct)
    {
        Rules.Require(
            key.StartsWith("staging/") && Guid.TryParse(key[8..], out _),
            "Invalid staging key."
        );
        Rules.Require(
            System.Text.RegularExpressions.Regex.IsMatch(block, "^[A-Za-z0-9+/=]{1,100}$"),
            "Invalid block identifier."
        );
        await Write(
            "blocks/"
                + key[8..]
                + "/"
                + Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(block))),
            content,
            ct
        );
    }

    public async Task Commit(string key, string[] blocks, CancellationToken ct)
    {
        Rules.Require(
            key.StartsWith("staging/") && Guid.TryParse(key[8..], out _),
            "Invalid staging key."
        );
        await using var target = File.Create(Resolve(key));
        foreach (var block in blocks)
        {
            await using var source = await Read(
                "blocks/"
                    + key[8..]
                    + "/"
                    + Convert.ToHexString(
                        SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(block))
                    ),
                ct
            );
            await source.CopyToAsync(target, ct);
        }
    }
}
