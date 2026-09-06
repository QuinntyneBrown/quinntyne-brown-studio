using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
namespace QuinntyneBrownStudio.AcceptanceTests;

public sealed class ClientResourceIsolationAcceptanceTests
{
    // Given another client's identifiers, when they are substituted directly into
    // reads and writes, then protected metadata/bytes and ownership changes are denied.
    [Fact]
    public async Task AC_L2_034_01_Client_identifiers_do_not_grant_gallery_photo_album_or_print_access()
    {
        using var factory = new StudioFactory(); var ownerId = Guid.NewGuid();
        var photo = await PrivatePhotoFixture.Seed(factory, ownerId);
        using var owner = await factory.Actor("Client", ownerId.ToString()); using var other = await factory.Actor("Client", Guid.NewGuid().ToString()); using var admin = await factory.Actor();
        var albumResponse = await owner.PostAsJsonAsync("/api/client/albums", new { name = "Secret album", photoIds = new[] { photo.Id } }); albumResponse.EnsureSuccessStatusCode();
        var albumId = (await albumResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        var optionResponse = await admin.PostAsJsonAsync("/api/admin/print-options", new { name = "Private selection", dimensions = "8x10", finish = "Matte", unitPrice = "10", enabled = true }); optionResponse.EnsureSuccessStatusCode();
        var option = (await optionResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        var lines = new[] { new { photoId = photo.Id, optionId = option, optionRevision = 1, quantity = 1 } };
        var printResponse = await owner.PostAsJsonAsync("/api/client/print-requests", new { idempotencyKey = "private-request", lines }); printResponse.EnsureSuccessStatusCode();
        var printId = (await printResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        foreach (var path in new[] { $"galleries/{photo.SessionId}", $"photos/{photo.Id}/preview", $"albums/{albumId}", $"print-requests/{printId}" })
        {
            var response = await other.GetAsync("/api/client/" + path); Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var text = await response.Content.ReadAsStringAsync(); Assert.DoesNotContain("Private portrait", text); Assert.DoesNotContain("Secret album", text);
        }
        var mutations = new[] {
            await other.PutAsJsonAsync($"/api/client/albums/{albumId}", new { name = "Stolen", photoIds = new[] { photo.Id }, expectedVersion = 1, clientId = ownerId }),
            await other.PostAsJsonAsync("/api/client/albums", new { name = "Stolen", photoIds = new[] { photo.Id }, clientId = ownerId }),
            await other.PostAsJsonAsync("/api/client/print-requests/preview", new { inputRevision = 1, lines, clientId = ownerId }),
            await other.PostAsJsonAsync("/api/client/print-requests", new { idempotencyKey = "stolen-request", lines, clientId = ownerId }) };
        foreach (var response in mutations) { Assert.Contains(response.StatusCode, new[] { HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.Forbidden }); Assert.DoesNotContain("Private portrait", await response.Content.ReadAsStringAsync()); }
        Assert.Equal("Secret album", (await owner.GetFromJsonAsync<JsonElement>($"/api/client/albums/{albumId}")).GetProperty("name").GetString());
        Assert.Single((await admin.GetFromJsonAsync<JsonElement[]>("/api/admin/print-requests"))!);
    }
}
