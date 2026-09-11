using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using QuinntyneBrownStudio.Infrastructure.Storage.Blog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace QuinntyneBrownStudio.AcceptanceTests;

// AC-L2-070-04: reuse imported upload, image generation, serving, and reference checks.
public sealed class BlogMediaAcceptanceTests
{
    [Fact]
    public async Task Images_render_with_variants_and_referenced_media_cannot_be_deleted()
    {
        var path = Path.Combine(Path.GetTempPath(), "qbs-blog-media-" + Guid.NewGuid().ToString("N"));
        try
        {
            await using var factory = new StudioFactory
            {
                ConfigurePersistence = services => services.PostConfigure<BlogStorageOptions>(o => o.Path = path)
            };
            using var admin = await factory.Actor();
            using var visitor = await factory.Actor(null);
            using var image = new Image<Rgba32>(700, 400);
            using var bytes = new MemoryStream();
            await image.SaveAsJpegAsync(bytes);
            using var upload = new MultipartFormDataContent();
            upload.Add(new ByteArrayContent(bytes.ToArray()), "file", "portrait.jpg");
            var response = await admin.PostAsync("/blog/api/digital-assets", upload);
            Assert.True(response.StatusCode == HttpStatusCode.Created, await response.Content.ReadAsStringAsync());
            var asset = await response.Content.ReadFromJsonAsync<JsonElement>();
            var assetId = asset.GetProperty("digitalAssetId").GetGuid();
            var url = asset.GetProperty("url").GetString()!;
            Assert.StartsWith("/blog/assets/", url);
            Assert.Equal(HttpStatusCode.OK, (await visitor.GetAsync(url)).StatusCode);
            using var variant = new HttpRequestMessage(HttpMethod.Get, url + "?w=320");
            variant.Headers.Accept.ParseAdd("image/webp");
            var resized = await visitor.SendAsync(variant);
            Assert.Equal("image/webp", resized.Content.Headers.ContentType!.MediaType);
            using var decoded = Image.Load(await resized.Content.ReadAsByteArrayAsync());
            Assert.Equal(320, decoded.Width);
            Assert.Equal(183, decoded.Height);
            using var browserImage = new HttpRequestMessage(HttpMethod.Get, url + "?w=320");
            browserImage.Headers.Accept.ParseAdd("image/avif,image/webp,image/*");
            var browserResponse = await visitor.SendAsync(browserImage);
            Assert.True((await browserResponse.Content.ReadAsByteArrayAsync()).Length > 0);
            var create = await admin.PostAsJsonAsync("/blog/api/articles", new
            {
                title = "Portrait with image", body = "Portrait story", @abstract = "Portrait", featuredImageId = assetId
            });
            Assert.Equal(HttpStatusCode.Created, create.StatusCode);
            Assert.Equal(HttpStatusCode.Conflict, (await admin.DeleteAsync($"/blog/api/digital-assets/{assetId}")).StatusCode);
            Assert.Equal(HttpStatusCode.OK, (await visitor.GetAsync(url)).StatusCode);
        }
        finally
        {
            if (Directory.Exists(path)) Directory.Delete(path, recursive: true);
        }
    }

    [Fact]
    public async Task Corrupt_image_is_rejected_without_leaving_files()
    {
        var path = Path.Combine(Path.GetTempPath(), "qbs-blog-media-" + Guid.NewGuid().ToString("N"));
        try
        {
            await using var factory = new StudioFactory
            {
                ConfigurePersistence = services => services.PostConfigure<BlogStorageOptions>(o => o.Path = path)
            };
            using var admin = await factory.Actor();
            using var upload = new MultipartFormDataContent();
            upload.Add(new ByteArrayContent([0xff, 0xd8, 0xff, 0x00, 0x00]), "file", "corrupt.jpg");
            var response = await admin.PostAsync("/blog/api/digital-assets", upload);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.True(!Directory.Exists(path) || Directory.GetFiles(path).Length == 0);
        }
        finally
        {
            if (Directory.Exists(path)) Directory.Delete(path, recursive: true);
        }
    }
}
