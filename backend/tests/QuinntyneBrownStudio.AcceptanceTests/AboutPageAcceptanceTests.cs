using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Enums;

namespace QuinntyneBrownStudio.AcceptanceTests;

// Acceptance tests: AC-L2-071-01 through AC-L2-071-04, and AC-L2-076-01 through AC-L2-076-03 for /about.
public sealed class AboutPageAcceptanceTests
{
    private const string DefaultHeading = "Photographs that feel like you.";
    private const string DefaultIntroduction =
        "A small Toronto studio photographing weddings, events, headshots, and family portraits the way they actually feel: unhurried, honest, and a little bit wild.";

    [Fact]
    public async Task AC_L2_071_01_About_renders_published_copy_fixed_sections_and_active_photographers()
    {
        await using var factory = new StudioFactory();
        using var admin = await factory.Actor();
        using var visitor = await factory.Actor(null);
        await PublishAbout(admin, "Photographs with room to breathe.", "Published introduction.", true);
        await AddPhotographer(admin, "Quinntyne Brown");
        await AddPhotographer(admin, "Mara Adeyemi");
        await AddPhotographer(admin, "Retired Photographer", active: false);

        var html = await Page(visitor);
        Assert.Contains("Photographs with room to breathe.", html);
        Assert.Contains("Published introduction.", html);
        Assert.Contains("A borrowed camera, a friend’s wedding, and a lot of listening.", html);
        foreach (var fixture in new[]
        {
            "Less posing. More being.", "Planning that feels like a conversation.", "Honest pricing, from the first estimate.",
            "A conversation", "The session", "Your gallery", "From hello to your gallery.",
        })
            Assert.Contains(fixture, html);
        var founder = html.IndexOf("Quinntyne Brown</h3>", StringComparison.Ordinal);
        var second = html.IndexOf("Mara Adeyemi</h3>", StringComparison.Ordinal);
        Assert.True(founder > 0 && second > founder, "Active photographers render in creation order.");
        Assert.Equal(1, Count(html, "Founder & lead photographer"));
        Assert.True(html.IndexOf("Founder & lead photographer", StringComparison.Ordinal) < second);
        Assert.DoesNotContain("Retired Photographer", html);
        Assert.Contains("2 photographers", html);
        foreach (var link in new[] { "href=\"/contact\"", "href=\"/portfolio\"", "href=\"/quote\"" })
            Assert.Contains(link, html);
    }

    [Fact]
    public async Task AC_L2_071_02_About_uses_default_copy_when_nothing_is_published_and_never_shows_drafts()
    {
        await using var factory = new StudioFactory();
        using var admin = await factory.Actor();
        using var visitor = await factory.Actor(null);

        var html = await Page(visitor);
        Assert.Contains(DefaultHeading, html);
        Assert.Contains(DefaultIntroduction, html);

        await PublishAbout(admin, "Secret draft heading", "Secret draft introduction", publish: false);
        html = await Page(visitor);
        Assert.Contains(DefaultHeading, html);
        Assert.DoesNotContain("Secret draft heading", html);
        Assert.DoesNotContain("Secret draft introduction", html);
    }

    [Fact]
    public async Task AC_L2_071_03_Published_changes_and_deactivation_appear_on_the_next_request()
    {
        await using var factory = new StudioFactory();
        using var admin = await factory.Actor();
        using var visitor = await factory.Actor(null);
        var version = await PublishAbout(admin, "First heading", "Introduction", true);
        var jonah = await AddPhotographer(admin, "Jonah Lindqvist");
        var html = await Page(visitor);
        Assert.Contains("First heading", html);
        Assert.Contains("Jonah Lindqvist", html);

        await PublishAbout(admin, "Second heading", "Introduction", true, version);
        var deactivate = await admin.PutAsJsonAsync(
            "/api/admin/photographers/" + jonah.GetProperty("id").GetGuid(),
            new { name = "Jonah Lindqvist", active = false, expectedVersion = jonah.GetProperty("version").GetInt64() }
        );
        Assert.True(deactivate.IsSuccessStatusCode, await deactivate.Content.ReadAsStringAsync());

        html = await Page(visitor);
        Assert.Contains("Second heading", html);
        Assert.DoesNotContain("First heading", html);
        Assert.DoesNotContain("Jonah Lindqvist", html);
    }

    [Fact]
    public async Task AC_L2_071_04_Empty_team_and_missing_gallery_show_placeholders_and_the_newest_gallery_is_the_hero()
    {
        await using var factory = new StudioFactory();
        using var visitor = await factory.Actor(null);

        var html = await Page(visitor);
        Assert.Contains("Introductions coming soon", html);
        Assert.Contains("A photograph is on its way", html);
        Assert.DoesNotContain("0 photographers", html);
        Assert.DoesNotContain("<img", html);

        await using var scope = factory.Services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IStudioStore>();
        var session = new Session { Name = "Cover session", ExpiresAt = DateTimeOffset.UtcNow.AddYears(1) };
        var older = new SessionPhoto { SessionId = session.Id, Name = "Older cover", State = PhotoState.Ready };
        var newer = new SessionPhoto { SessionId = session.Id, Name = "Newer cover", State = PhotoState.Ready };
        var unpublished = new SessionPhoto { SessionId = session.Id, Name = "Draft cover", State = PhotoState.Ready };
        await store.Run(
            "fixture",
            async tx =>
            {
                await tx.Save(session, 0);
                await tx.Save(older, 0);
                await tx.Save(newer, 0);
                await tx.Save(unpublished, 0);
                await tx.Save(new PublicGallery
                {
                    Title = "Ordinary magic", Slug = "ordinary-magic", PhotoIds = [older.Id], Published = true,
                    PublishedAt = DateTimeOffset.UtcNow.AddDays(-10),
                }, 0);
                await tx.Save(new PublicGallery
                {
                    Title = "In good company", Slug = "in-good-company", PhotoIds = [newer.Id], Published = true,
                    PublishedAt = DateTimeOffset.UtcNow.AddDays(-1),
                }, 0);
                await tx.Save(new PublicGallery
                {
                    Title = "Not yet public", Slug = "not-yet-public", PhotoIds = [unpublished.Id], Published = false,
                    PublishedAt = DateTimeOffset.UtcNow,
                }, 0);
                return true;
            }
        );

        html = await Page(visitor);
        Assert.Contains($"src=\"/api/public/galleries/in-good-company/photos/{newer.Id}\"", html);
        Assert.Contains("In good company", html);
        Assert.DoesNotContain("ordinary-magic", html);
        Assert.DoesNotContain("not-yet-public", html);
        Assert.DoesNotContain("A photograph is on its way", html);
    }

    [Fact]
    public async Task AC_L2_076_01_About_carries_search_metadata_and_the_trailing_slash_redirects_permanently()
    {
        await using var factory = new StudioFactory();
        using var visitor = await factory.Actor(null);
        var html = await Page(visitor);
        Assert.Contains("<title>About", html);
        Assert.Contains("<meta name=\"description\" content=\"", html);
        Assert.Contains("<link rel=\"canonical\" href=\"https://localhost:7443/about\" />", html);
        Assert.Contains("<meta property=\"og:url\" content=\"https://localhost:7443/about\" />", html);
        Assert.Contains("<meta property=\"og:type\" content=\"website\" />", html);
        Assert.Contains("\"@type\": \"AboutPage\"", html);

        var redirect = await visitor.GetAsync("/about/?x=1");
        Assert.Equal(HttpStatusCode.PermanentRedirect, redirect.StatusCode);
        Assert.Equal("/about?x=1", redirect.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task AC_L2_076_02_The_marketing_shell_links_every_page_and_marks_the_current_one()
    {
        await using var factory = new StudioFactory();
        using var visitor = await factory.Actor(null);
        var html = await Page(visitor);
        var navigation = Between(html, "aria-label=\"Main navigation\"", "</nav>");
        var expected = new[]
        {
            "href=\"/portfolio\">Portfolio", "href=\"/services\">Services", "href=\"/about\" aria-current=\"page\">About",
            "href=\"/blog\">Blog", "href=\"/prints\">Prints", "href=\"/promotions\">Packages", "href=\"/contact\">Contact",
            "href=\"/quote\">Find your quote", "href=\"/client/login\">Client login",
        };
        var position = -1;
        foreach (var link in expected)
        {
            var next = navigation.IndexOf(link, StringComparison.Ordinal);
            Assert.True(next > position, $"Expected {link} after position {position} in: {navigation}");
            position = next;
        }
        var footer = Between(html, "aria-label=\"Footer navigation\"", "</nav>");
        foreach (var link in new[]
        {
            "href=\"/about\">About the studio", "href=\"/contact\">Get in touch", "href=\"/portfolio\">Our work",
            "href=\"/blog\">Blog", "href=\"/client/login\">Client access",
        })
            Assert.Contains(link, footer);
        Assert.Contains("class=\"marketing-blog-backdrop\"", html);
        Assert.Contains("data-close-label=\"Close menu\"", html);

        var blog = await visitor.GetStringAsync("/blog");
        Assert.Contains("href=\"/blog\" aria-current=\"page\">Blog", blog);
        Assert.Contains("href=\"/about\">About", blog);
        Assert.DoesNotContain("href=\"/about\" aria-current", blog);
    }

    [Fact]
    public async Task AC_L2_076_03_A_repeated_request_answers_304_until_the_content_changes()
    {
        await using var factory = new StudioFactory();
        using var admin = await factory.Actor();
        using var visitor = await factory.Actor(null);
        var version = await PublishAbout(admin, "Initial heading", "Introduction", true);

        var first = await visitor.GetAsync("/about");
        var etag = first.Headers.ETag;
        Assert.NotNull(etag);
        Assert.True(first.Headers.CacheControl?.NoCache == true, first.Headers.CacheControl?.ToString());

        using var conditional = new HttpRequestMessage(HttpMethod.Get, "/about");
        conditional.Headers.TryAddWithoutValidation("If-None-Match", etag!.ToString());
        var unchanged = await visitor.SendAsync(conditional);
        Assert.Equal(HttpStatusCode.NotModified, unchanged.StatusCode);
        Assert.Equal("", await unchanged.Content.ReadAsStringAsync());

        await PublishAbout(admin, "Changed heading", "Introduction", true, version);
        using var repeated = new HttpRequestMessage(HttpMethod.Get, "/about");
        repeated.Headers.TryAddWithoutValidation("If-None-Match", etag.ToString());
        var changed = await visitor.SendAsync(repeated);
        Assert.Equal(HttpStatusCode.OK, changed.StatusCode);
        Assert.NotEqual(etag.ToString(), changed.Headers.ETag?.ToString());
        Assert.Contains("Changed heading", await changed.Content.ReadAsStringAsync());
    }

    private static async Task<long> PublishAbout(HttpClient admin, string heading, string body, bool publish, long expectedVersion = 0)
    {
        var response = await admin.PutAsJsonAsync("/api/admin/content/about", new { heading, body, publish, expectedVersion });
        Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("version").GetInt64();
    }

    private static async Task<JsonElement> AddPhotographer(HttpClient admin, string name, bool active = true)
    {
        var response = await admin.PostAsJsonAsync("/api/admin/photographers", new { name, active });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    private static async Task<string> Page(HttpClient visitor)
    {
        var response = await visitor.GetAsync("/about");
        var html = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK, $"{response.StatusCode}: {html}");
        Assert.Contains("text/html", response.Content.Headers.ContentType!.ToString());
        // Razor encodes punctuation such as dashes and apostrophes as entities; assertions read the decoded text.
        return System.Net.WebUtility.HtmlDecode(html);
    }

    private static string Between(string html, string start, string end)
    {
        var from = html.IndexOf(start, StringComparison.Ordinal);
        Assert.True(from >= 0, $"Missing {start}");
        var to = html.IndexOf(end, from, StringComparison.Ordinal);
        return html[from..to];
    }

    private static int Count(string html, string value) =>
        (html.Length - html.Replace(value, "").Length) / value.Length;
}
