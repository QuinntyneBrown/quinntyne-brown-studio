using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using QuinntyneBrownStudio.Application.Blog.Articles.Commands;

namespace QuinntyneBrownStudio.AcceptanceTests;

// Acceptance tests: AC-L2-078-01, AC-L2-078-03, AC-L2-078-04 (OD-14, the relaunch gate).
public sealed class LaunchGateAcceptanceTests
{
    private static StudioFactory Gated() =>
        new() { Settings = new Dictionary<string, string?> { ["Launch:ComingSoon"] = "true" } };

    // Given the gate is configured, when the launch state is read, then it applies to anonymous
    // visitors only; an unconfigured studio never gates anyone.
    [Fact]
    public async Task AC_L2_078_01_The_launch_state_gates_anonymous_visitors_only_while_configured()
    {
        await using var gated = Gated();
        using var visitor = await gated.Actor(null);
        using var admin = await gated.Actor();
        using var client = await gated.Actor("Client");
        Assert.True(await ComingSoon(visitor));
        Assert.False(await ComingSoon(admin));
        Assert.False(await ComingSoon(client));

        await using var open = new StudioFactory();
        using var anonymous = await open.Actor(null);
        Assert.False(await ComingSoon(anonymous));
    }

    // Given the gate applies, when the API renders the marketing shell, then the navigation offers
    // only About, Blog, Contact, the quote and client login, the footer drops the portfolio link,
    // the brand leads to the blog, and About and Contact still render; a signed-in account gets
    // the whole shell back.
    [Fact]
    public async Task AC_L2_078_03_The_server_rendered_shell_hides_the_gated_pages_from_anonymous_visitors()
    {
        await using var factory = Gated();
        using var visitor = await factory.Actor(null);
        using var admin = await factory.Actor();

        var html = await visitor.GetStringAsync("/blog");
        var navigation = Between(html, "aria-label=\"Main navigation\"", "</nav>");
        var position = -1;
        foreach (var link in new[]
        {
            "href=\"/about\">About", "href=\"/blog\" aria-current=\"page\">Blog", "href=\"/contact\">Contact",
            "href=\"/quote\">Find your quote", "href=\"/client/login\">Client login",
        })
        {
            var next = navigation.IndexOf(link, StringComparison.Ordinal);
            Assert.True(next > position, $"Expected {link} after position {position} in: {navigation}");
            position = next;
        }
        foreach (var gated in new[] { "href=\"/portfolio\"", "href=\"/services\"", "href=\"/prints\"", "href=\"/promotions\"" })
            Assert.DoesNotContain(gated, html);
        Assert.Contains("class=\"marketing-blog-brand\" href=\"/blog\"", html);
        Assert.DoesNotContain("class=\"marketing-blog-brand\" href=\"/\"", html);
        var footer = Between(html, "aria-label=\"Footer navigation\"", "</nav>");
        Assert.Contains("href=\"/about\">About the studio", footer);
        Assert.Contains("href=\"/contact\">Get in touch", footer);
        Assert.DoesNotContain("Our work", footer);
        foreach (var path in new[] { "/about", "/contact" })
        {
            var page = await visitor.GetAsync(path);
            Assert.Equal(HttpStatusCode.OK, page.StatusCode);
            Assert.DoesNotContain("href=\"/portfolio\"", await page.Content.ReadAsStringAsync());
        }

        var signedIn = await admin.GetStringAsync("/blog");
        Assert.Contains("href=\"/portfolio\">Portfolio", signedIn);
        Assert.Contains("href=\"/promotions\">Packages", signedIn);
        Assert.Contains("class=\"marketing-blog-brand\" href=\"/\"", signedIn);
        Assert.Contains("href=\"/portfolio\">Our work", signedIn);

        // A copy cached while gated never stands in for the full shell after signing in.
        foreach (var path in new[] { "/blog", "/about", "/contact" })
        {
            var anonymous = await visitor.GetAsync(path);
            var etag = anonymous.Headers.ETag!.ToString();
            using var revalidate = new HttpRequestMessage(HttpMethod.Get, path);
            revalidate.Headers.TryAddWithoutValidation("If-None-Match", etag);
            var afterSignIn = await admin.SendAsync(revalidate);
            Assert.Equal(HttpStatusCode.OK, afterSignIn.StatusCode);
            Assert.Contains("href=\"/portfolio\"", await afterSignIn.Content.ReadAsStringAsync());
            using var repeat = new HttpRequestMessage(HttpMethod.Get, path);
            repeat.Headers.TryAddWithoutValidation("If-None-Match", etag);
            Assert.Equal(HttpStatusCode.NotModified, (await visitor.SendAsync(repeat)).StatusCode);
        }
    }

    // Given the gate is configured and the blog is empty, when the API starts, then the coming-soon
    // article is published once and reachable from the listing, its page and the feed; a blog that
    // already holds an article receives nothing, and an ungated empty blog stays empty.
    [Fact]
    public async Task AC_L2_078_04_A_coming_soon_article_is_published_into_an_empty_blog_only_while_gated()
    {
        await using var gated = Gated();
        using var visitor = await gated.Actor(null);
        using var admin = await gated.Actor();
        var listing = await visitor.GetStringAsync("/blog");
        Assert.Contains("Coming soon", listing);
        Assert.Contains("href=\"/blog/articles/coming-soon\"", listing);
        Assert.DoesNotContain("No articles yet", listing);
        var article = await visitor.GetAsync("/blog/articles/coming-soon");
        Assert.Equal(HttpStatusCode.OK, article.StatusCode);
        var body = await article.Content.ReadAsStringAsync();
        Assert.Contains("The studio is relaunching", body);
        Assert.Contains("href=\"/quote\"", body);
        Assert.Contains("/blog/articles/coming-soon", await visitor.GetStringAsync("/blog/feed.xml"));
        using (var scope = gated.Services.CreateScope())
            Assert.False(await scope.ServiceProvider.GetRequiredService<ISender>().Send(new PublishLaunchArticleCommand()));
        Assert.Equal(1, await ArticleCount(admin));

        await using var open = new StudioFactory();
        using var anonymous = await open.Actor(null);
        using var author = await open.Actor();
        Assert.Contains("No articles yet", await anonymous.GetStringAsync("/blog"));
        var draft = await author.PostAsJsonAsync("/blog/api/articles", new
        {
            title = "First light", body = "A draft.", @abstract = "A draft.", featuredImageId = (Guid?)null
        });
        Assert.Equal(HttpStatusCode.Created, draft.StatusCode);
        using (var scope = open.Services.CreateScope())
            Assert.False(await scope.ServiceProvider.GetRequiredService<ISender>().Send(new PublishLaunchArticleCommand()));
        Assert.Equal(1, await ArticleCount(author));
        Assert.DoesNotContain("coming-soon", await anonymous.GetStringAsync("/blog"));
    }

    private static async Task<bool> ComingSoon(HttpClient client)
    {
        var response = await client.GetAsync("/api/public/launch");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("comingSoon").GetBoolean();
    }

    private static async Task<int> ArticleCount(HttpClient admin)
    {
        var page = await admin.GetFromJsonAsync<JsonElement>("/blog/api/articles?page=1&pageSize=50");
        return page.GetProperty("totalCount").GetInt32();
    }

    private static string Between(string html, string start, string end)
    {
        var from = html.IndexOf(start, StringComparison.Ordinal);
        Assert.True(from >= 0, $"Missing {start}");
        var to = html.IndexOf(end, from, StringComparison.Ordinal);
        return html[from..to];
    }
}
