using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace QuinntyneBrownStudio.AcceptanceTests;

// Acceptance tests: AC-L2-075-01 through AC-L2-075-03 (administration API).
public sealed class StudioDetailsAcceptanceTests
{
    [Fact]
    public async Task AC_L2_075_01_Saved_studio_details_are_read_back_and_start_empty()
    {
        await using var factory = new StudioFactory();
        using var admin = await factory.Actor();

        var empty = await admin.GetFromJsonAsync<JsonElement>("/api/admin/studio-details");
        Assert.Equal(0, empty.GetProperty("version").GetInt64());
        Assert.Equal(JsonValueKind.Null, empty.GetProperty("email").ValueKind);

        var saved = await admin.PutAsJsonAsync(
            "/api/admin/studio-details",
            new
            {
                email = "hello@example.test",
                phone = "416-555-0100",
                hours = "Monday – Saturday · 09:00 – 18:00",
                replyNote = "Within two working days",
                expectedVersion = 0,
            }
        );
        Assert.True(saved.IsSuccessStatusCode, await saved.Content.ReadAsStringAsync());
        var details = await admin.GetFromJsonAsync<JsonElement>("/api/admin/studio-details");
        Assert.Equal("hello@example.test", details.GetProperty("email").GetString());
        Assert.Equal("416-555-0100", details.GetProperty("phone").GetString());
        Assert.Equal("Monday – Saturday · 09:00 – 18:00", details.GetProperty("hours").GetString());
        Assert.Equal("Within two working days", details.GetProperty("replyNote").GetString());
        Assert.Equal(1, details.GetProperty("version").GetInt64());
    }

    [Theory]
    [InlineData("email", "not-an-address")]
    [InlineData("email", "long")]
    [InlineData("phone", "long")]
    [InlineData("hours", "long")]
    [InlineData("replyNote", "long")]
    public async Task AC_L2_075_02_Invalid_details_are_rejected_beside_their_field_and_previous_details_remain(string field, string kind)
    {
        await using var factory = new StudioFactory();
        using var admin = await factory.Actor();
        (await admin.PutAsJsonAsync(
            "/api/admin/studio-details",
            new { email = "hello@example.test", phone = "416-555-0100", hours = "Weekdays", replyNote = "Soon", expectedVersion = 0 }
        )).EnsureSuccessStatusCode();

        var limit = field switch { "email" => 254, "phone" => 50, _ => 200 };
        var value = kind == "long" ? (field == "email" ? new string('a', limit - 11) + "@example.test" : new string('x', limit + 1)) : kind;
        var input = new Dictionary<string, object?>
        {
            ["email"] = "hello@example.test",
            ["phone"] = "416-555-0100",
            ["hours"] = "Weekdays",
            ["replyNote"] = "Soon",
            ["expectedVersion"] = 1,
            [field] = value,
        };
        var rejected = await admin.PutAsJsonAsync("/api/admin/studio-details", input);
        Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);
        var problem = await rejected.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(problem.GetProperty("errors").TryGetProperty(field, out _), problem.ToString());

        var details = await admin.GetFromJsonAsync<JsonElement>("/api/admin/studio-details");
        Assert.Equal("hello@example.test", details.GetProperty("email").GetString());
        Assert.Equal("416-555-0100", details.GetProperty("phone").GetString());
        Assert.Equal("Weekdays", details.GetProperty("hours").GetString());
        Assert.Equal("Soon", details.GetProperty("replyNote").GetString());
        Assert.Equal(1, details.GetProperty("version").GetInt64());
    }

    [Fact]
    public async Task AC_L2_075_02_Blank_details_are_stored_as_absent_rather_than_placeholders()
    {
        await using var factory = new StudioFactory();
        using var admin = await factory.Actor();
        (await admin.PutAsJsonAsync(
            "/api/admin/studio-details",
            new { email = "  ", phone = "", hours = (string?)null, replyNote = " Within a day ", expectedVersion = 0 }
        )).EnsureSuccessStatusCode();
        var details = await admin.GetFromJsonAsync<JsonElement>("/api/admin/studio-details");
        Assert.Equal(JsonValueKind.Null, details.GetProperty("email").ValueKind);
        Assert.Equal(JsonValueKind.Null, details.GetProperty("phone").ValueKind);
        Assert.Equal(JsonValueKind.Null, details.GetProperty("hours").ValueKind);
        Assert.Equal("Within a day", details.GetProperty("replyNote").GetString());
    }

    [Fact]
    public async Task AC_L2_075_03_A_stale_save_is_rejected_as_a_conflict_and_the_newer_details_survive()
    {
        await using var factory = new StudioFactory();
        using var first = await factory.Actor();
        using var second = await factory.Actor();
        (await first.PutAsJsonAsync(
            "/api/admin/studio-details",
            new { email = "first@example.test", expectedVersion = 0 }
        )).EnsureSuccessStatusCode();
        (await second.PutAsJsonAsync(
            "/api/admin/studio-details",
            new { email = "second@example.test", expectedVersion = 1 }
        )).EnsureSuccessStatusCode();

        var stale = await first.PutAsJsonAsync(
            "/api/admin/studio-details",
            new { email = "stale@example.test", expectedVersion = 1 }
        );
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        var details = await first.GetFromJsonAsync<JsonElement>("/api/admin/studio-details");
        Assert.Equal("second@example.test", details.GetProperty("email").GetString());
        Assert.Equal(2, details.GetProperty("version").GetInt64());
    }
}
