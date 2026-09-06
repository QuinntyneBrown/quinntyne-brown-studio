using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Enums;

namespace QuinntyneBrownStudio.AcceptanceTests;

public sealed class PrintReviewAcceptanceTests
{
    // Given assigned photos and saved prices, when a client reviews selections,
    // then the server prices the current revision without creating a request.
    [Fact]
    public async Task P06_AC_L2_035_01_AC_L2_064_01_Review_uses_current_prices_and_creates_no_request()
    {
        using var factory = new StudioFactory();
        var clientId = Guid.NewGuid();
        var (photo, option) = await Seed(factory, clientId);
        using var client = await factory.Actor("Client", clientId.ToString());
        using var admin = await factory.Actor();
        var response = await client.PostAsJsonAsync("/api/client/print-requests/preview", new
        {
            inputRevision = 7,
            lines = new[] { new { photoId = photo.Id, optionId = option.Id, quantity = 3, unitPrice = "0.01", amount = "0.03" } }
        });
        response.EnsureSuccessStatusCode();
        var preview = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(7, preview.GetProperty("inputRevision").GetInt64());
        Assert.Equal("76.65", preview.GetProperty("total").GetString());
        var line = preview.GetProperty("lines")[0];
        Assert.Equal("25.55", line.GetProperty("unitPrice").GetString());
        Assert.Equal(1, line.GetProperty("optionRevision").GetInt64());
        Assert.Equal("Matte", line.GetProperty("finish").GetString());
        Assert.Empty((await admin.GetFromJsonAsync<JsonElement[]>("/api/admin/print-requests"))!);
        (await admin.PutAsJsonAsync($"/api/admin/print-options/{option.Id}", new { name = "Portrait", dimensions = "8x10", finish = "Matte", unitPrice = "30.10", enabled = true, expectedVersion = 1 })).EnsureSuccessStatusCode();
        var changed = await client.PostAsJsonAsync("/api/client/print-requests/preview", new { inputRevision = 8, lines = new[] { new { photoId = photo.Id, optionId = option.Id, quantity = 3 } } });
        changed.EnsureSuccessStatusCode();
        var refreshed = await changed.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("90.30", refreshed.GetProperty("total").GetString());
        Assert.Equal(2, refreshed.GetProperty("lines")[0].GetProperty("optionRevision").GetInt64());
    }

    // Given a review draft, when invalid quantities or inaccessible photos are used,
    // then review is rejected and protected photo metadata is absent.
    [Theory]
    [InlineData(0, false, HttpStatusCode.BadRequest)]
    [InlineData(-1, false, HttpStatusCode.BadRequest)]
    [InlineData(1, true, HttpStatusCode.NotFound)]
    public async Task P06_AC_L2_034_01_Review_rejects_invalid_or_unassigned_selections(int quantity, bool otherClient, HttpStatusCode expected)
    {
        using var factory = new StudioFactory();
        var clientId = Guid.NewGuid();
        var (photo, option) = await Seed(factory, clientId);
        using var client = await factory.Actor("Client", (otherClient ? Guid.NewGuid() : clientId).ToString());
        var response = await client.PostAsJsonAsync("/api/client/print-requests/preview", new { inputRevision = 1, lines = new[] { new { photoId = photo.Id, optionId = option.Id, quantity } } });
        Assert.Equal(expected, response.StatusCode);
        Assert.DoesNotContain(photo.Name, await response.Content.ReadAsStringAsync());
    }

    private static Task<(SessionPhoto, PrintOption)> Seed(StudioFactory factory, Guid clientId) =>
        factory.Services.GetRequiredService<IStudioStore>().Run("fixture", async tx =>
        {
            var session = new Session { Name = "Assigned session", ClientIds = [clientId], ExpiresAt = DateTimeOffset.UtcNow.AddMonths(12) };
            await tx.Save(session, 0);
            var photo = new SessionPhoto { Name = "private-portrait.jpg", SessionId = session.Id, State = PhotoState.Ready };
            await tx.Save(photo, 0);
            var option = new PrintOption { Name = "Portrait", Dimensions = "8x10", Finish = "Matte", UnitPrice = 25.55m, Enabled = true };
            await tx.Save(option, 0);
            return (photo, option);
        });
}
