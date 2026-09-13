using System.Net.Http.Json;
using System.Text.Json;
using System.Xml.Linq;

namespace QuinntyneBrownStudio.AcceptanceTests;

// Acceptance tests: AC-L2-077-01.
public sealed class SiteDiscoveryAcceptanceTests
{
    private static readonly XNamespace Sitemap = "http://www.sitemaps.org/schemas/sitemap/0.9";

    [Fact]
    public async Task AC_L2_077_01_Robots_advertises_one_site_sitemap_that_lists_the_marketing_pages_with_publication_dates()
    {
        await using var factory = new StudioFactory();
        using var admin = await factory.Actor();
        using var visitor = await factory.Actor(null);
        foreach (var key in new[] { "home", "about", "contact" })
            await Publish(admin, key);
        var slug = await PublishArticle(admin);

        var robots = await visitor.GetAsync("/robots.txt");
        Assert.Contains("text/plain", robots.Content.Headers.ContentType!.ToString());
        var directives = await robots.Content.ReadAsStringAsync();
        Assert.Contains("User-agent: *", directives);
        Assert.Contains("Allow: /", directives);
        foreach (var path in new[] { "/admin/", "/client/", "/api/", "/blog/admin/", "/blog/api/" })
            Assert.Contains($"Disallow: {path}", directives);
        Assert.DoesNotContain("Disallow: /about", directives);
        Assert.DoesNotContain("Disallow: /contact", directives);
        Assert.Contains("Sitemap: https://localhost:7443/sitemap.xml", directives);
        Assert.DoesNotContain("Sitemap: https://localhost:7443/blog/sitemap.xml", directives);

        var sitemap = await visitor.GetAsync("/sitemap.xml");
        Assert.Contains("xml", sitemap.Content.Headers.ContentType!.ToString());
        var urls = Urls(await sitemap.Content.ReadAsStringAsync());
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        Assert.Equal(today, urls["https://localhost:7443/"]);
        Assert.Equal(today, urls["https://localhost:7443/about"]);
        Assert.Equal(today, urls["https://localhost:7443/contact"]);
        Assert.True(urls.ContainsKey("https://localhost:7443/blog"));
        Assert.True(urls.ContainsKey($"https://localhost:7443/blog/articles/{slug}"));
    }

    [Fact]
    public async Task AC_L2_077_01_Unpublished_pages_are_listed_without_a_modification_date()
    {
        await using var factory = new StudioFactory();
        using var visitor = await factory.Actor(null);
        var urls = Urls(await visitor.GetStringAsync("/sitemap.xml"));
        foreach (var address in new[] { "https://localhost:7443/", "https://localhost:7443/about", "https://localhost:7443/contact" })
        {
            Assert.True(urls.ContainsKey(address), address);
            Assert.Null(urls[address]);
        }
    }

    private static Dictionary<string, string?> Urls(string xml) =>
        XDocument.Parse(xml)
            .Root!.Elements(Sitemap + "url")
            .ToDictionary(
                url => url.Element(Sitemap + "loc")!.Value,
                url => url.Element(Sitemap + "lastmod")?.Value
            );

    private static async Task Publish(HttpClient admin, string key)
    {
        var response = await admin.PutAsJsonAsync(
            $"/api/admin/content/{key}",
            new { heading = $"Published {key}", body = "Copy", publish = true, expectedVersion = 0 }
        );
        Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
    }

    private static async Task<string> PublishArticle(HttpClient admin)
    {
        var create = await admin.PostAsJsonAsync(
            "/blog/api/articles",
            new { title = "Sitemap story", body = "# Light\nA story.", @abstract = "A story", featuredImageId = (Guid?)null }
        );
        Assert.True(create.IsSuccessStatusCode, await create.Content.ReadAsStringAsync());
        var article = await create.Content.ReadFromJsonAsync<JsonElement>();
        using var publish = new HttpRequestMessage(HttpMethod.Patch, $"/blog/api/articles/{article.GetProperty("articleId").GetGuid()}/publish")
        {
            Content = JsonContent.Create(new { published = true }),
        };
        publish.Headers.TryAddWithoutValidation("If-Match", create.Headers.ETag!.ToString());
        var published = await admin.SendAsync(publish);
        Assert.True(published.IsSuccessStatusCode, await published.Content.ReadAsStringAsync());
        return article.GetProperty("slug").GetString()!;
    }
}
