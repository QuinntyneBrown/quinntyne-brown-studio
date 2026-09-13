using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Enums;

namespace QuinntyneBrownStudio.AcceptanceTests;

// Acceptance tests: AC-L2-074-01 (administration API) and AC-L2-074-04.
public sealed class InquiryInboxAcceptanceTests
{
    [Fact]
    public async Task AC_L2_074_01_The_inbox_lists_inquiries_newest_first_shows_every_value_and_records_a_review()
    {
        await using var factory = new StudioFactory();
        using var admin = await factory.Actor();
        var (oldest, middle, newest) = await Seed(factory);

        var listed = await admin.GetFromJsonAsync<JsonElement>("/api/admin/inquiries");
        var ids = listed.EnumerateArray().Select(x => x.GetProperty("id").GetGuid()).ToArray();
        Assert.Equal(new[] { newest.Id, middle.Id, oldest.Id }, ids);
        var first = listed.EnumerateArray().First();
        Assert.Equal("Priya Raman", first.GetProperty("name").GetString());
        Assert.Equal("Wedding", first.GetProperty("interest").GetString());
        Assert.Equal("Submitted", first.GetProperty("state").GetString());
        Assert.Equal("QB-IN-1043", first.GetProperty("reference").GetString());
        Assert.True(first.TryGetProperty("submittedAt", out _));

        var one = await admin.GetFromJsonAsync<JsonElement>($"/api/admin/inquiries/{middle.Id}");
        Assert.Equal("Daniel Okafor", one.GetProperty("name").GetString());
        Assert.Equal("daniel@example.test", one.GetProperty("email").GetString());
        Assert.Equal(JsonValueKind.Null, one.GetProperty("phone").ValueKind);
        Assert.Equal("Headshot", one.GetProperty("interest").GetString());
        Assert.Equal("Looking for two headshot looks. <b>Bold?</b>", one.GetProperty("message").GetString());
        Assert.Equal(1, one.GetProperty("version").GetInt64());

        var reviewed = await admin.PostAsJsonAsync($"/api/admin/inquiries/{middle.Id}/review", new { expectedVersion = 1 });
        Assert.True(reviewed.IsSuccessStatusCode, await reviewed.Content.ReadAsStringAsync());
        var result = await reviewed.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Reviewed", result.GetProperty("state").GetString());
        Assert.Equal("00000000-0000-0000-0000-000000000001", result.GetProperty("reviewedBy").GetString());
        Assert.NotEqual(JsonValueKind.Null, result.GetProperty("reviewedAt").ValueKind);
        Assert.Equal(2, result.GetProperty("version").GetInt64());

        var stale = await admin.PostAsJsonAsync($"/api/admin/inquiries/{middle.Id}/review", new { expectedVersion = 1 });
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);

        var submitted = await admin.GetFromJsonAsync<JsonElement>("/api/admin/inquiries?state=Submitted");
        Assert.Equal(new[] { newest.Id, oldest.Id }, submitted.EnumerateArray().Select(x => x.GetProperty("id").GetGuid()).ToArray());
        var reviewedOnly = await admin.GetFromJsonAsync<JsonElement>("/api/admin/inquiries?state=Reviewed");
        Assert.Equal(new[] { middle.Id }, reviewedOnly.EnumerateArray().Select(x => x.GetProperty("id").GetGuid()).ToArray());
        Assert.Equal(HttpStatusCode.NotFound, (await admin.GetAsync($"/api/admin/inquiries/{Guid.NewGuid()}")).StatusCode);
    }

    [Fact]
    public async Task AC_L2_074_04_Anonymous_visitors_and_clients_are_denied_and_change_nothing()
    {
        await using var factory = new StudioFactory();
        using var anonymous = await factory.Actor(null);
        using var client = await factory.Actor("Client");
        var (_, middle, _) = await Seed(factory);

        foreach (var (actor, expected) in new[] { (anonymous, HttpStatusCode.Unauthorized), (client, HttpStatusCode.Forbidden) })
        {
            Assert.Equal(expected, (await actor.GetAsync("/api/admin/inquiries")).StatusCode);
            Assert.Equal(expected, (await actor.GetAsync($"/api/admin/inquiries/{middle.Id}")).StatusCode);
            Assert.Equal(expected, (await actor.PostAsJsonAsync($"/api/admin/inquiries/{middle.Id}/review", new { expectedVersion = 1 })).StatusCode);
        }

        await using var scope = factory.Services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IStudioStore>();
        var unchanged = (await store.Run("test", tx => tx.Get<Inquiry>(middle.Id)))!;
        Assert.Equal("Submitted", unchanged.State);
        Assert.Equal(1, unchanged.Version);
        Assert.Null(unchanged.ReviewedBy);
    }

    private static async Task<(Inquiry Oldest, Inquiry Middle, Inquiry Newest)> Seed(StudioFactory factory)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IStudioStore>();
        var now = DateTimeOffset.UtcNow;
        var oldest = Inquiry("QB-IN-1041", "Amara Bell", "amara@example.test", "416-555-0101", ServiceKind.Event, "An anniversary dinner.", now.AddDays(-3));
        var middle = Inquiry("QB-IN-1042", "Daniel Okafor", "daniel@example.test", null, ServiceKind.Headshot, "Looking for two headshot looks. <b>Bold?</b>", now.AddDays(-2));
        var newest = Inquiry("QB-IN-1043", "Priya Raman", "priya@example.test", "416-555-0166", ServiceKind.Wedding, "A small September wedding.", now.AddDays(-1));
        await store.Run("fixture", async tx =>
        {
            // Saved out of submission order so the listing must sort rather than echo storage order.
            await tx.Save(middle, 0);
            await tx.Save(newest, 0);
            await tx.Save(oldest, 0);
            return true;
        });
        return (oldest, middle, newest);
    }

    private static Inquiry Inquiry(string reference, string name, string email, string? phone, ServiceKind interest, string message, DateTimeOffset at) =>
        new()
        {
            Reference = reference,
            Name = name,
            Email = email,
            Phone = phone,
            Interest = interest,
            Message = message,
            ConsentAt = at,
            SubmittedAt = at,
        };
}
