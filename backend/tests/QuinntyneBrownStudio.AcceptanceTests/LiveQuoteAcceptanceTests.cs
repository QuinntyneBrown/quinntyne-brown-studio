using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Enums;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;

namespace QuinntyneBrownStudio.AcceptanceTests;

// Given/When/Then: docs/implementation/live-quote-slice.md Q-01 through Q-08.
public sealed class LiveQuoteAcceptanceTests
{
    private static QuoteInput Input() => new()
    {
        Service = ServiceKind.Wedding,
        StartsAt = DateTimeOffset.Parse("2027-06-01T10:00:00-04:00"),
        EndsAt = DateTimeOffset.Parse("2027-06-01T11:15:00-04:00"),
        InputRevision = 19,
        Locations = [new() { Location = new() { Label = "Venue A", Latitude = 43.7m, Longitude = -79.3m }, ParkingAmount = 1.005m },
                     new() { Location = new() { Label = "Venue B", Latitude = 43.8m, Longitude = -79.2m }, ParkingAmount = 2.005m }],
    };

    private static async Task Configure(HttpClient admin, decimal? rate = 100.004m)
    {
        (await admin.PostAsJsonAsync("/api/admin/studios", new
        {
            name = "Base",
            resolvedAddress = new { label = "Base", latitude = 43.6, longitude = -79.4 },
            hourlyFee = "25",
            enabled = true,
            isBase = true,
        })).EnsureSuccessStatusCode();
        (await admin.PutAsJsonAsync("/api/admin/rates", new
        {
            serviceRates = new Dictionary<string, decimal?> { ["Wedding"] = rate, ["Event"] = rate, ["Headshot"] = rate, ["FamilyPortrait"] = rate },
            costRates = new { travel = "0.75", assistant = "40", equipment = "30", lunch = "15" },
            expectedVersion = 0,
        })).EnsureSuccessStatusCode();
    }

    [Theory]
    [InlineData(ServiceKind.Wedding)]
    [InlineData(ServiceKind.Event)]
    [InlineData(ServiceKind.Headshot)]
    [InlineData(ServiceKind.FamilyPortrait)]
    public async Task Q01_AC_L2_055_01_AC_L2_056_01_Rounds_lines_and_routes_ordered_round_trip(ServiceKind service)
    {
        using var root = new StudioFactory();
        var routes = new QuoteRoutes();
        using var factory = root.WithWebHostBuilder(b => b.ConfigureServices(s => { s.AddSingleton<IRouteDistanceService>(routes); s.AddSingleton<IClock>(new QuoteClock()); }));
        using var admin = await Actor(factory);
        await Configure(admin);
        var input = Input(); input.Service = service; input.AssistantCount = 2; input.EquipmentUnits = 1; input.LunchCount = 2;
        var response = await admin.PostAsJsonAsync("/api/public/quotes/calculate", input);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        // 125.005 -> 125.01; 12.345 * .75 -> 9.26; assistants 100; equipment 30; lunches 30; parking 1.01 + 2.01.
        Assert.Equal("297.29", result.GetProperty("total").GetProperty("amount").GetString());
        Assert.Equal(19, result.GetProperty("inputRevision").GetInt32());
        Assert.True(result.GetProperty("configurationRevision").GetInt64() > 0);
        Assert.Equal(new[] { "Base", "Venue A", "Venue B", "Base" }, routes.LastRoute.Select(x => x.Label));
        input.AssistantCount = input.EquipmentUnits = input.LunchCount = 0; input.Locations = [input.Locations[0]];
        var removed = await admin.PostAsJsonAsync("/api/public/quotes/calculate", input);
        removed.EnsureSuccessStatusCode();
        Assert.Equal("135.28", (await removed.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("total").GetProperty("amount").GetString());
    }

    [Theory]
    [InlineData("null-stop")]
    [InlineData("negative-count")]
    [InlineData("invalid-service")]
    [InlineData("quarter-hour")]
    [InlineData("wrong-offset")]
    [InlineData("past")]
    [InlineData("negative-parking")]
    [InlineData("overflow")]
    public async Task Q03_AC_L2_012_01_Invalid_input_is_a_controlled_error(string scenario)
    {
        using var root = new StudioFactory();
        using var factory = root.WithWebHostBuilder(b => b.ConfigureServices(s => s.AddSingleton<IClock>(new QuoteClock())));
        using var admin = await Actor(factory); await Configure(admin);
        var input = Input();
        switch (scenario)
        {
            case "null-stop": input.Locations = [null!]; break;
            case "negative-count": input.AssistantCount = -1; break;
            case "invalid-service": input.Service = (ServiceKind)99; break;
            case "quarter-hour": input.EndsAt = input.EndsAt.AddMinutes(1); break;
            case "wrong-offset": input.StartsAt = DateTimeOffset.Parse("2027-06-01T10:00:00-05:00"); break;
            case "past": input.StartsAt = input.StartsAt.AddYears(-10); input.EndsAt = input.EndsAt.AddYears(-10); break;
            case "negative-parking": input.Locations[0].ParkingAmount = -1; break;
            case "overflow": input.Locations[0].ParkingAmount = decimal.MaxValue; break;
        }
        var response = await admin.PostAsJsonAsync("/api/public/quotes/calculate", input);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Q04_AC_L2_012_02_Invalid_provider_distance_never_becomes_a_quote()
    {
        using var root = new StudioFactory(); var routes = new QuoteRoutes { Distance = -1 };
        using var factory = root.WithWebHostBuilder(b => b.ConfigureServices(s => s.AddSingleton<IRouteDistanceService>(routes)));
        using var admin = await Actor(factory); await Configure(admin);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, (await admin.PostAsJsonAsync("/api/public/quotes/calculate", Input())).StatusCode);
    }

    [Theory]
    [InlineData("code", "Code", "87.41", false)]
    [InlineData("tie", "Code", "98.33", false)]
    [InlineData("advance", "Advance", "98.33", false)]
    [InlineData("weekday", "Weekday", "98.33", false)]
    [InlineData("unknown", "Advance", "98.33", true)]
    [InlineData("expired", "Advance", "98.33", true)]
    [InlineData("disabled", "Advance", "98.33", true)]
    [InlineData("none", null, "109.26", false)]
    public async Task Q06_AC_L2_014_01_AC_L2_017_02_Discounts_are_current_identified_and_not_stacked(string scenario, string? kind, string total, bool codeError)
    {
        using var root = new StudioFactory();
        using var factory = root.WithWebHostBuilder(b => b.ConfigureServices(s => { s.AddSingleton<IRouteDistanceService>(new QuoteRoutes()); s.AddSingleton<IClock>(new QuoteClock()); }));
        using var admin = await Actor(factory); await Configure(admin);
        (await admin.PutAsJsonAsync("/api/admin/discounts", new
        {
            advanceRule = new { enabled = scenario != "weekday" && scenario != "none", percentage = "10", threshold = 90 },
            weekdayRule = new { enabled = scenario != "none", percentage = "10", weekdays = new[] { "Tuesday" } },
            codeRules = new[] { new { code = "SAVE", enabled = scenario != "disabled", percentage = scenario == "code" ? "20" : "10", validTo = scenario == "expired" ? "2027-03-02" : "2027-03-03" } },
            expectedVersion = 0,
        })).EnsureSuccessStatusCode();
        var input = Input(); input.EndsAt = input.StartsAt.AddHours(1); input.Locations = [input.Locations[0]]; input.Locations[0].ParkingAmount = 0;
        input.Code = scenario is "code" or "tie" or "expired" or "disabled" ? " save " : scenario == "unknown" ? "MISSING" : null;
        var response = await admin.PostAsJsonAsync("/api/public/quotes/calculate", input); response.EnsureSuccessStatusCode();
        var quote = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(total, quote.GetProperty("total").GetProperty("amount").GetString());
        Assert.Equal(kind, quote.GetProperty("discount").GetProperty("kind").GetString());
        Assert.Equal(codeError, quote.GetProperty("discount").GetProperty("codeError").ValueKind != JsonValueKind.Null);
        // Removing the code and moving inside the advance threshold to a non-slow day removes eligibility.
        input.Code = null; input.StartsAt = input.StartsAt.AddDays(-1); input.EndsAt = input.EndsAt.AddDays(-1);
        var changed = await admin.PostAsJsonAsync("/api/public/quotes/calculate", input); changed.EnsureSuccessStatusCode();
        Assert.Equal("0.00", (await changed.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("discount").GetProperty("percentage").GetString());
    }

    [Fact]
    public async Task Q08_AC_L2_012_02_Saved_rate_changes_and_zero_are_used_but_unset_is_unavailable()
    {
        using var root = new StudioFactory();
        using var factory = root.WithWebHostBuilder(b => b.ConfigureServices(s => s.AddSingleton<IClock>(new QuoteClock())));
        using var admin = await Actor(factory); await Configure(admin);
        var input = Input(); input.Locations = [input.Locations[0]]; input.Locations[0].ParkingAmount = 0;
        for (var version = 1; version <= 3; version++)
        {
            decimal? rate = version == 1 ? 200 : version == 2 ? 0 : null;
            (await admin.PutAsJsonAsync("/api/admin/rates", new
            {
                serviceRates = new Dictionary<string, decimal?> { ["Wedding"] = rate },
                costRates = new { travel = "0", equipment = "0", lunch = "0", assistant = "0" },
                expectedVersion = version
            })).EnsureSuccessStatusCode();
            var response = await admin.PostAsJsonAsync("/api/public/quotes/calculate", input);
            if (version == 3) Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
            else { response.EnsureSuccessStatusCode(); Assert.Equal(version == 1 ? "250.00" : "0.00", (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("total").GetProperty("amount").GetString()); }
        }
    }

    [Fact]
    public async Task Q07_AC_L2_022_01_Quote_reads_buffered_availability_without_creating_a_session()
    {
        using var root = new StudioFactory();
        using var factory = root.WithWebHostBuilder(b => b.ConfigureServices(s => s.AddSingleton<IClock>(new QuoteClock())));
        using var admin = await Actor(factory); await Configure(admin);
        var created = await admin.PostAsJsonAsync("/api/admin/photographers", new { name = "Photographer", active = true }); created.EnsureSuccessStatusCode();
        var id = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        var input = Input();
        for (var attempt = 0; attempt < 2; attempt++)
        {
            (await admin.PutAsJsonAsync($"/api/admin/photographers/{id}/schedule", new
            {
                workingWindows = new[] { new { startsAt = "2027-06-01T09:30:00-04:00", endsAt = "2027-06-01T11:45:00-04:00" } },
                unavailableWindows = attempt == 0 ? Array.Empty<object>() : [new { startsAt = "2027-06-01T11:30:00-04:00", endsAt = "2027-06-01T11:45:00-04:00" }],
                expectedVersion = attempt,
            })).EnsureSuccessStatusCode();
            var response = await admin.PostAsJsonAsync("/api/public/quotes/calculate", input); response.EnsureSuccessStatusCode();
            Assert.Equal(attempt == 0, (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("availability").GetProperty("available").GetBoolean());
        }
        Assert.Empty((await admin.GetFromJsonAsync<JsonElement[]>("/api/admin/sessions"))!);
    }

    [Theory]
    [InlineData("equipment", "279.94")]
    [InlineData("lunch", "279.94")]
    [InlineData("assistant", "216.94")]
    [InlineData("parking", "306.03")]
    [InlineData("studio", "278.81")]
    public async Task Q01Optional_AC_L2_009_02_AC_L2_055_01_Each_removed_cost_is_absent(string removed, string expected)
    {
        using var root = new StudioFactory();
        using var factory = root.WithWebHostBuilder(b => b.ConfigureServices(s => { s.AddSingleton<IRouteDistanceService>(new QuoteRoutes()); s.AddSingleton<IClock>(new QuoteClock()); }));
        using var admin = await Actor(factory); await Configure(admin);
        var studios = await admin.GetFromJsonAsync<JsonElement[]>("/api/public/studios");
        var id = Assert.Single(studios!).GetProperty("id").GetGuid();
        var input = Input(); input.AssistantCount = 2; input.EquipmentUnits = 1; input.LunchCount = 2;
        input.Locations[0].StudioId = input.Locations[1].StudioId = id;
        input.Locations[0].StudioHours = 1.25m; input.Locations[1].StudioHours = .5m;
        (await admin.PutAsJsonAsync("/api/admin/discounts", new { advanceRule = new { enabled = true, percentage = "10", threshold = 90 }, expectedVersion = 0 })).EnsureSuccessStatusCode();
        var complete = await admin.PostAsJsonAsync("/api/public/quotes/calculate", input); complete.EnsureSuccessStatusCode();
        Assert.Equal("306.94", (await complete.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("total").GetProperty("amount").GetString());
        switch (removed)
        {
            case "equipment": input.EquipmentUnits = 0; break;
            case "lunch": input.LunchCount = 0; break;
            case "assistant": input.AssistantCount = 0; break;
            case "parking": input.Locations[0].ParkingAmount = 0; break;
            case "studio": input.Locations[0].StudioId = null; input.Locations[0].StudioHours = 0; break;
        }
        var response = await admin.PostAsJsonAsync("/api/public/quotes/calculate", input); response.EnsureSuccessStatusCode();
        var quote = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(expected, quote.GetProperty("total").GetProperty("amount").GetString());
        Assert.DoesNotContain(quote.GetProperty("lines").EnumerateArray(), line => line.GetProperty("kind").GetString() == removed &&
            (line.GetProperty("locationIndex").ValueKind == JsonValueKind.Null || line.GetProperty("locationIndex").GetInt32() == 0));
    }

    [Fact]
    public async Task Q08_AC_L2_009_01_Disabled_studios_are_not_selectable_and_cannot_be_quoted()
    {
        using var root = new StudioFactory();
        using var factory = root.WithWebHostBuilder(b => b.ConfigureServices(s => s.AddSingleton<IClock>(new QuoteClock())));
        using var admin = await Actor(factory); await Configure(admin);
        var created = await admin.PostAsJsonAsync("/api/admin/studios", new { name = "Local rental", resolvedAddress = new { label = "Rental", latitude = 43.7, longitude = -79.3 }, hourlyFee = "40", enabled = true, isBase = false });
        created.EnsureSuccessStatusCode(); var id = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        Assert.Equal(2, (await admin.GetFromJsonAsync<JsonElement[]>("/api/public/studios"))!.Length);
        (await admin.PutAsJsonAsync($"/api/admin/studios/{id}", new { name = "Local rental", resolvedAddress = new { label = "Rental", latitude = 43.7, longitude = -79.3 }, hourlyFee = "40", enabled = false, isBase = false, expectedVersion = 1 })).EnsureSuccessStatusCode();
        Assert.Single((await admin.GetFromJsonAsync<JsonElement[]>("/api/public/studios"))!);
        var input = Input(); input.Locations[0].StudioId = id; input.Locations[0].StudioHours = 1;
        Assert.Equal(HttpStatusCode.BadRequest, (await admin.PostAsJsonAsync("/api/public/quotes/calculate", input)).StatusCode);
    }

    // Given saved advance rules, when the threshold changes across this session's
    // lead time, then the next HTTP quote uses the newly saved threshold.
    [Fact]
    public async Task AC_L2_015_03_Changed_saved_threshold_controls_the_next_quote()
    {
        using var root = new StudioFactory();
        using var factory = root.WithWebHostBuilder(b => b.ConfigureServices(s => s.AddSingleton<IClock>(new QuoteClock())));
        using var admin = await Actor(factory);
        await Configure(admin);
        foreach (var (threshold, version, expectedKind) in new[] { (91, 0, (string?)null), (89, 1, "Advance"), (91, 2, (string?)null) })
        {
            (await admin.PutAsJsonAsync("/api/admin/discounts", new
            {
                advanceRule = new { enabled = true, percentage = 10, threshold },
                weekdayRule = new { enabled = false, percentage = 0, weekdays = Array.Empty<string>() },
                codeRules = Array.Empty<object>(), expectedVersion = version,
            })).EnsureSuccessStatusCode();
            var response = await admin.PostAsJsonAsync("/api/public/quotes/calculate", Input());
            response.EnsureSuccessStatusCode();
            var quote = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(expectedKind, quote.GetProperty("discount").GetProperty("kind").GetString());
        }
    }

    internal static async Task<HttpClient> Actor(Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> factory)
    {
        var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });
        client.DefaultRequestHeaders.Add("X-Test-Actor", "Administrator:00000000-0000-0000-0000-000000000001");
        var token = await client.GetFromJsonAsync<JsonElement>("/api/auth/antiforgery");
        client.DefaultRequestHeaders.Add("X-XSRF-TOKEN", token.GetProperty("requestToken").GetString());
        return client;
    }
}
