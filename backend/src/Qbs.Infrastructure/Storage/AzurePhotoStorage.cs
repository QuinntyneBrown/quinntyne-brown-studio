using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Configuration;
using Qbs.Application;
using Qbs.Domain;

namespace Qbs.Infrastructure;

public sealed class AzurePhotoStorage(IConfiguration config) : IPhotoStorage
{
    private BlobServiceClient Service =>
        config["Azure:StorageConnectionString"] is { } connection
            ? new(connection)
            : new(
                new Uri(
                    config["Azure:BlobEndpoint"]
                        ?? throw new StudioException(503, "Photo storage is not configured.")
                ),
                new DefaultAzureCredential()
            );
    private BlobContainerClient Container => Service.GetBlobContainerClient("photos");

    public async Task<StorageGrant> Grant(string key, CancellationToken ct)
    {
        var blob = Container.GetBlobClient(key);
        var expiry = DateTimeOffset.UtcNow.AddMinutes(15);
        var sas = new BlobSasBuilder
        {
            BlobContainerName = "photos",
            BlobName = key,
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow.AddMinutes(-1),
            ExpiresOn = expiry,
        };
        sas.SetPermissions(BlobSasPermissions.Create | BlobSasPermissions.Write);
        if (blob.CanGenerateSasUri)
            return new(blob.GenerateSasUri(sas).ToString(), expiry);
        var service = Service;
        var delegation = await service.GetUserDelegationKeyAsync(sas.StartsOn, expiry, ct);
        return new(
            new BlobUriBuilder(blob.Uri)
            {
                Sas = sas.ToSasQueryParameters(delegation.Value, service.AccountName),
            }
                .ToUri()
                .ToString(),
            expiry
        );
    }

    public async Task<Stream> Read(string key, CancellationToken ct)
    {
        try
        {
            return await Container.GetBlobClient(key).OpenReadAsync(cancellationToken: ct);
        }
        catch (RequestFailedException e) when (e.Status == 404)
        {
            throw new StudioException(404, "Photo bytes are unavailable.");
        }
    }

    public async Task Write(string key, Stream content, CancellationToken ct) =>
        await Container.GetBlobClient(key).UploadAsync(content, true, ct);

    public async Task Delete(string key, CancellationToken ct) =>
        await Container.GetBlobClient(key).DeleteIfExistsAsync(cancellationToken: ct);

    public async Task Finalize(SessionPhoto photo, CancellationToken ct)
    {
        var original = Container.GetBlobClient(photo.OriginalKey);
        if (await original.ExistsAsync(ct))
        {
            await using var existing = await original.OpenReadAsync(cancellationToken: ct);
            await PhotoIntegrity.Validate(existing, photo, ct);
            return;
        }
        var staging = Container.GetBlobClient(photo.StagingKey);
        var props = await staging.GetPropertiesAsync(cancellationToken: ct);
        await using var source = await staging.OpenReadAsync(
            new BlobOpenReadOptions(false)
            {
                Conditions = new BlobRequestConditions { IfMatch = props.Value.ETag },
            },
            ct
        );
        await PhotoIntegrity.Validate(source, photo, ct);
        await original.UploadAsync(
            source,
            new BlobUploadOptions
            {
                Conditions = new BlobRequestConditions { IfNoneMatch = ETag.All },
            },
            ct
        );
    }
}
