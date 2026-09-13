using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace QuinntyneBrownStudio.AcceptanceTests;

// Acceptance tests: AC-L2-072-01, AC-L2-072-02, and AC-L2-076-01 through AC-L2-076-03 for /contact.
public sealed class ContactPageAcceptanceTests
{
    [Fact]
    public async Task AC_L2_072_01_Contact_renders_published_copy_configured_details_base_studio_first_and_the_advance_rule()
    {
        await using var factory = new StudioFactory();
        using var admin = await factory.Actor();
        using var visitor = await factory.Actor(null);
        await Publish(admin, "Something beautiful starts with hello.", "Tell us what you have in mind.");
        (await admin.PutAsJsonAsync(
            "/api/admin/studio-details",
            new
            {
                email = "hello@example.test",
                phone = "416-555-0100",
                hours = "Monday – Saturday · 09:00 – 18:00",
                replyNote = "Within two working days",
                expectedVersion = 0,
            }
        )).EnsureSuccessStatusCode();
        await AddStudio(admin, "The White Room", "48 Example Avenue, Hamilton", isBase: false);
        await AddStudio(admin, "Daylight Studio", "120 Sample Street, Toronto", isBase: true);
        var discounts = await admin.PutAsJsonAsync(
            "/api/admin/discounts",
            new
            {
                advanceRule = new { enabled = true, percentage = 10, threshold = 90 },
                weekdayRule = new { enabled = false, percentage = 0 },
                codeRules = Array.Empty<object>(),
                expectedVersion = 0,
            }
        );
        Assert.True(discounts.IsSuccessStatusCode, await discounts.Content.ReadAsStringAsync());

        var html = await Page(visitor);
        Assert.Contains("Something beautiful starts with hello.", html);
        Assert.Contains("Tell us what you have in mind.", html);
        Assert.Contains("href=\"mailto:hello@example.test\"", html);
        Assert.Contains("href=\"tel:+14165550100\"", html);
        Assert.Contains("Monday – Saturday · 09:00 – 18:00", html);
        Assert.Contains("Within two working days", html);
        Assert.Contains("We usually reply within two working days.", html);
        var daylight = html.IndexOf("Daylight Studio</h3>", StringComparison.Ordinal);
        var whiteRoom = html.IndexOf("The White Room</h3>", StringComparison.Ordinal);
        Assert.True(daylight > 0 && whiteRoom > daylight, "The base studio is listed first.");
        Assert.Contains("120 Sample Street, Toronto", html);
        Assert.Contains("48 Example Avenue, Hamilton", html);
        Assert.Contains("Two studio spaces. Any location you love.", html);
        Assert.Contains("On location</h3>", html);
        Assert.Contains("How far ahead should we book?", html);
        Assert.Contains("Booking at least 90 days ahead also earns 10% off.", html);
        Assert.Contains("Do you travel?", html);
        Assert.Contains("How do we receive our photographs?", html);
        Assert.Contains("href=\"/quote\"", html);
        Assert.Contains("<dt>Studio</dt>", html);
        Assert.Contains("<form", html);
        Assert.DoesNotContain("Studio spaces coming soon", html);
    }

    [Fact]
    public async Task AC_L2_072_02_Unconfigured_contact_page_omits_rows_and_shows_the_empty_studios_state()
    {
        await using var factory = new StudioFactory();
        using var visitor = await factory.Actor(null);
        var html = await Page(visitor);
        Assert.Contains("Something beautiful", html);
        Assert.Contains("Studio spaces coming soon", html);
        Assert.Contains("On location</h3>", html);
        Assert.Contains("On location, anywhere you love.", html);
        Assert.Contains("On location, by appointment", html);
        foreach (var row in new[] { "<dt>Email</dt>", "<dt>Phone</dt>", "<dt>Hours</dt>", "<dt>Replies</dt>" })
            Assert.DoesNotContain(row, html);
        Assert.DoesNotContain("also earns", html);
        Assert.Contains("We usually reply within two working days.", html);
        Assert.Contains("<form", html);
        Assert.Contains("name=\"name\"", html);
        Assert.Contains("name=\"email\"", html);
        Assert.Contains("name=\"message\"", html);
        Assert.Contains("name=\"consent\"", html);
        Assert.Contains("Send your message", html);
    }

    [Fact]
    public async Task AC_L2_076_01_Contact_carries_search_metadata_and_the_trailing_slash_redirects_permanently()
    {
        await using var factory = new StudioFactory();
        using var visitor = await factory.Actor(null);
        var html = await Page(visitor);
        Assert.Contains("<title>Contact", html);
        Assert.Contains("<meta name=\"description\" content=\"", html);
        Assert.Contains("<link rel=\"canonical\" href=\"https://localhost:7443/contact\" />", html);
        Assert.Contains("<meta property=\"og:url\" content=\"https://localhost:7443/contact\" />", html);
        Assert.Contains("\"@type\": \"ContactPage\"", html);
        Assert.Contains("href=\"/contact\" aria-current=\"page\">Contact", html);

        var redirect = await visitor.GetAsync("/contact/?sent=QB-IN-1041");
        Assert.Equal(HttpStatusCode.PermanentRedirect, redirect.StatusCode);
        Assert.Equal("/contact?sent=QB-IN-1041", redirect.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task AC_L2_076_03_A_repeated_request_answers_304_until_the_studio_details_change()
    {
        await using var factory = new StudioFactory();
        using var admin = await factory.Actor();
        using var visitor = await factory.Actor(null);

        var first = await visitor.GetAsync("/contact");
        var etag = first.Headers.ETag;
        Assert.NotNull(etag);
        Assert.True(first.Headers.CacheControl?.NoCache == true, first.Headers.CacheControl?.ToString());

        using var conditional = new HttpRequestMessage(HttpMethod.Get, "/contact");
        conditional.Headers.TryAddWithoutValidation("If-None-Match", etag!.ToString());
        var unchanged = await visitor.SendAsync(conditional);
        Assert.Equal(HttpStatusCode.NotModified, unchanged.StatusCode);
        Assert.Equal("", await unchanged.Content.ReadAsStringAsync());

        (await admin.PutAsJsonAsync(
            "/api/admin/studio-details",
            new { phone = "416-555-0199", expectedVersion = 0 }
        )).EnsureSuccessStatusCode();
        using var repeated = new HttpRequestMessage(HttpMethod.Get, "/contact");
        repeated.Headers.TryAddWithoutValidation("If-None-Match", etag.ToString());
        var changed = await visitor.SendAsync(repeated);
        Assert.Equal(HttpStatusCode.OK, changed.StatusCode);
        Assert.NotEqual(etag.ToString(), changed.Headers.ETag?.ToString());
        Assert.Contains("416-555-0199", await changed.Content.ReadAsStringAsync());
    }

    private static async Task Publish(HttpClient admin, string heading, string body)
    {
        var response = await admin.PutAsJsonAsync(
            "/api/admin/content/contact",
            new { heading, body, publish = true, expectedVersion = 0 }
        );
        Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
    }

    private static async Task AddStudio(HttpClient admin, string name, string address, bool isBase)
    {
        var response = await admin.PostAsJsonAsync(
            "/api/admin/studios",
            new
            {
                name,
                hourlyFee = "95",
                enabled = true,
                isBase,
                resolvedAddress = new { label = address, latitude = 43.65, longitude = -79.38 },
            }
        );
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    private static async Task<string> Page(HttpClient visitor)
    {
        var response = await visitor.GetAsync("/contact");
        var html = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK, $"{response.StatusCode}: {html}");
        Assert.Contains("text/html", response.Content.Headers.ContentType!.ToString());
        // Razor encodes punctuation such as dashes and apostrophes as entities; assertions read the decoded text.
        return System.Net.WebUtility.HtmlDecode(html);
    }
}
