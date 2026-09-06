using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Qbs.Application.Photos;
using Qbs.Application.Ports;
using Qbs.Domain.Entities;
using Qbs.Domain.Enums;
using Qbs.Infrastructure.Processing;
using SkiaSharp;

namespace Qbs.AcceptanceTests;

public sealed class PhotoProcessingTests
{
    [Fact]
    public async Task AC_L2_028_02_Invalid_uploaded_bytes_are_reported_as_rejected_without_a_preview_job()
    {
        using var factory = new StudioFactory();
        await using var scope = factory.Services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IStudioStore>();
        var photo = await store.Run(
            "fixture",
            async tx =>
            {
                var session = new Session();
                await tx.Save(session, 0);
                var p = new SessionPhoto
                {
                    SessionId = session.Id,
                    BatchId = Guid.NewGuid(),
                    Name = "invalid.jpg",
                    Size = 3,
                    Sha256 = new string('0', 64),
                    State = PhotoState.Uploading,
                    StagingKey = $"staging/{Guid.NewGuid()}",
                    OriginalKey = $"originals/{Guid.NewGuid()}",
                };
                await tx.Save(p, 0);
                return p;
            }
        );
        await scope
            .ServiceProvider.GetRequiredService<IPhotoStorage>()
            .Write(photo.StagingKey, new MemoryStream([1, 2, 3]), CancellationToken.None);
        await scope
            .ServiceProvider.GetRequiredService<PhotoWorkflows>()
            .Finalize(photo.BatchId, photo.Id, CancellationToken.None);
        var saved = (await store.Run("fixture", tx => tx.Get<SessionPhoto>(photo.Id)))!;
        Assert.Equal(PhotoState.Rejected, saved.State);
        Assert.NotNull(saved.Failure);
        Assert.Empty(await store.Run("fixture", tx => tx.List<BackgroundJob>()));
    }

    [Fact]
    public async Task AC_L2_027_01_AC_L2_028_02_AC_L2_029_01_AC_L2_059_01_Original_integrity_and_repeated_finalization()
    {
        using var factory = new StudioFactory();
        using var admin = await factory.Actor();
        var created = await admin.PostAsJsonAsync(
            "/api/admin/sessions",
            new
            {
                name = "Portrait session",
                service = "Headshot",
                startsAt = "2027-06-01T10:00:00-04:00",
                endsAt = "2027-06-01T11:00:00-04:00",
            }
        );
        created.EnsureSuccessStatusCode();
        var session = (await created.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("id")
            .GetGuid();
        using var bitmap = new SKBitmap(16, 24);
        bitmap.Erase(SKColors.Olive);
        using var jpeg = bitmap.Encode(SKEncodedImageFormat.Jpeg, 90);
        var bytes = jpeg.ToArray();
        var manifest = await admin.PostAsJsonAsync(
            $"/api/admin/sessions/{session}/uploads",
            new
            {
                files = new[]
                {
                    new
                    {
                        clientFileId = "camera-1",
                        name = "portrait.jpg",
                        size = bytes.Length,
                        sha256 = Convert.ToHexString(SHA256.HashData(bytes)),
                    },
                },
            }
        );
        manifest.EnsureSuccessStatusCode();
        var batch = await manifest.Content.ReadFromJsonAsync<JsonElement>();
        var batchId = batch.GetProperty("id").GetGuid();
        var photoId = batch.GetProperty("files")[0].GetProperty("photoId").GetGuid();
        var grant = batch
            .GetProperty("grants")
            .GetProperty(photoId.ToString())
            .GetProperty("url")
            .GetString();
        var blockId = Convert.ToBase64String(Encoding.ASCII.GetBytes("00000000"));
        (
            await admin.PutAsync(
                grant + "?comp=block&blockid=" + Uri.EscapeDataString(blockId),
                new ByteArrayContent(bytes)
            )
        ).EnsureSuccessStatusCode();
        (
            await admin.PutAsync(
                grant + "?comp=blocklist",
                new StringContent(
                    $"<BlockList><Latest>{blockId}</Latest></BlockList>",
                    Encoding.UTF8,
                    "application/xml"
                )
            )
        ).EnsureSuccessStatusCode();
        var path = $"/api/admin/uploads/{batchId}/files/{photoId}/complete";
        Assert.Equal(
            HttpStatusCode.Accepted,
            (await admin.PostAsJsonAsync(path, new { })).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.Accepted,
            (await admin.PostAsJsonAsync(path, new { })).StatusCode
        );
        await using var scope = factory.Services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IStudioStore>();
        var jobs = await store.Run("fixture", tx => tx.List<BackgroundJob>());
        Assert.Single(jobs);
        await scope
            .ServiceProvider.GetRequiredService<JobProcessor>()
            .Process(jobs[0].Id, CancellationToken.None);
        var photos = await admin.GetFromJsonAsync<JsonElement>(
            $"/api/admin/sessions/{session}/photos"
        );
        Assert.Equal(
            "Ready",
            Assert
                .Single(photos.GetProperty("photos").EnumerateArray())
                .GetProperty("state")
                .GetString()
        );
        var preview = await admin.GetAsync($"/api/admin/photos/{photoId}/preview");
        preview.EnsureSuccessStatusCode();
        Assert.Equal("image/jpeg", preview.Content.Headers.ContentType?.MediaType);
        Assert.Contains("no-store", preview.Headers.CacheControl!.ToString());
        var thumbnail = await admin.GetAsync($"/api/admin/photos/{photoId}/preview?thumbnail=true");
        thumbnail.EnsureSuccessStatusCode();
        using var thumbnailBitmap = SKBitmap.Decode(await thumbnail.Content.ReadAsByteArrayAsync());
        Assert.InRange(Math.Max(thumbnailBitmap.Width, thumbnailBitmap.Height), 1, 480);
        var photo = (await store.Run("fixture", tx => tx.Get<SessionPhoto>(photoId)))!;
        await using var original = await scope
            .ServiceProvider.GetRequiredService<IPhotoStorage>()
            .Read(photo.OriginalKey, CancellationToken.None);
        Assert.Equal(SHA256.HashData(bytes), await SHA256.HashDataAsync(original));
        // An old staging URL cannot replace the finalized original.
        (
            await admin.PutAsync(
                grant + "?comp=blocklist",
                new StringContent("<BlockList/>", Encoding.UTF8, "application/xml")
            )
        ).EnsureSuccessStatusCode();
        await using var stillOriginal = await scope
            .ServiceProvider.GetRequiredService<IPhotoStorage>()
            .Read(photo.OriginalKey, CancellationToken.None);
        Assert.Equal(bytes.Length, stillOriginal.Length);
    }

    [Fact]
    public async Task AC_L2_030_01_AC_L2_030_02_Advisory_analysis_does_not_publish_or_assign_photos()
    {
        using var factory = new StudioFactory();
        using var admin = await factory.Actor();
        await using var scope = factory.Services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IStudioStore>();
        var photo = await store.Run(
            "fixture",
            async tx =>
            {
                var session = new Session { Name = "Unpublished session" };
                await tx.Save(session, 0);
                var p = new SessionPhoto
                {
                    SessionId = session.Id,
                    Name = "photo.jpg",
                    State = PhotoState.Ready,
                    PreviewKey = $"previews/{Guid.NewGuid()}.jpg",
                };
                await tx.Save(p, 0);
                return p;
            }
        );
        await scope
            .ServiceProvider.GetRequiredService<IPhotoStorage>()
            .Write(photo.PreviewKey!, new MemoryStream([255, 216, 255]), CancellationToken.None);
        var response = await admin.PostAsJsonAsync(
            $"/api/admin/sessions/{photo.SessionId}/analysis",
            new { photoIds = new[] { photo.Id } }
        );
        response.EnsureSuccessStatusCode();
        var batchId = (await response.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("id")
            .GetGuid();
        var jobs = await store.Run("fixture", tx => tx.List<BackgroundJob>());
        await scope
            .ServiceProvider.GetRequiredService<JobProcessor>()
            .Process(Assert.Single(jobs).Id, CancellationToken.None);
        var status = await admin.GetFromJsonAsync<JsonElement>($"/api/admin/analysis/{batchId}");
        Assert.Equal("Succeeded", status.GetProperty("photos")[0].GetProperty("state").GetString());
        Assert.Empty(await store.Run("fixture", tx => tx.List<PublicGallery>()));
        Assert.Empty(
            (await store.Run("fixture", tx => tx.Get<Session>(photo.SessionId)))!.ClientIds
        );
    }
}
