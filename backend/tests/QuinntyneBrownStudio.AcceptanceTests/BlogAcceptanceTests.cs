using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace QuinntyneBrownStudio.AcceptanceTests;

// Acceptance tests: AC-L2-070-01, AC-L2-070-03, AC-L2-070-06 through AC-L2-070-11.
public sealed class BlogAcceptanceTests
{
    [Fact]
    public async Task Public_blog_is_server_rendered_and_missing_articles_return_404()
    {
        await using var factory = new StudioFactory();
        using var visitor = await factory.Actor(null);
        var response = await visitor.GetAsync("/blog");
        Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("<html", html);
        Assert.Contains("Quinntyne Brown Studio", html);
        Assert.Contains("class=\"marketing-blog\"", html);
        Assert.Contains("From the studio", html);
        Assert.Contains("No articles yet", html);
        Assert.Contains("href=\"/blog/search\"", html);
        // AC-L2-070-11: the overlay menu ships its dimming backdrop and its close icon.
        Assert.Contains("class=\"marketing-blog-backdrop\"", html);
        Assert.Contains("class=\"marketing-blog-menu-close\"", html);
        Assert.Contains("data-close-label=\"Close menu\"", html);
        Assert.DoesNotContain("aria-controls=\"mobile-menu\">Menu</button>", html);
        Assert.Equal(HttpStatusCode.NotFound, (await visitor.GetAsync("/blog/articles/missing")).StatusCode);
    }

    // AC-L2-070-01, AC-L2-070-02: exercise imported handlers through HTTP.
    [Fact]
    public async Task Administrator_can_publish_search_update_unpublish_and_delete_without_disclosing_drafts()
    {
        await using var factory = new StudioFactory();
        using var admin = await factory.Actor();
        using var visitor = await factory.Actor(null);
        var create = await admin.PostAsJsonAsync("/blog/api/articles", new
        {
            title = "Wedding light", body = "# Golden hour\nA portrait story. <script>alert('bad')</script>",
            @abstract = "Working with sunset light", featuredImageId = (Guid?)null
        });
        Assert.True(create.StatusCode == HttpStatusCode.Created, await create.Content.ReadAsStringAsync());
        var article = await create.Content.ReadFromJsonAsync<JsonElement>();
        var id = article.GetProperty("articleId").GetGuid();
        var slug = article.GetProperty("slug").GetString();
        Assert.DoesNotContain("<script", article.GetProperty("bodyHtml").GetString());
        Assert.Equal(HttpStatusCode.NotFound, (await visitor.GetAsync($"/blog/articles/{slug}")).StatusCode);
        Assert.DoesNotContain("Wedding light", await visitor.GetStringAsync("/blog"));
        using var publish = new HttpRequestMessage(HttpMethod.Patch, $"/blog/api/articles/{id}/publish")
        {
            Content = JsonContent.Create(new { published = true })
        };
        publish.Headers.TryAddWithoutValidation("If-Match", create.Headers.ETag!.ToString());
        var published = await admin.SendAsync(publish);
        Assert.True(published.IsSuccessStatusCode, await published.Content.ReadAsStringAsync());
        foreach (var path in new[] { "/blog", "/blog/articles" })
        {
            var listing = await visitor.GetStringAsync(path);
            Assert.Contains("class=\"marketing-blog\"", listing);
            Assert.Contains("class=\"blog-grid\"", listing);
            Assert.Contains("class=\"blog-post\"", listing);
            Assert.Contains($"href=\"/blog/articles/{slug}\"", listing);
            Assert.Contains("class=\"blog-post-image-placeholder\"", listing);
            Assert.DoesNotContain("Working with sunset light", listing);
            Assert.DoesNotContain("min read", listing);
        }
        Assert.Contains("Golden hour", await visitor.GetStringAsync($"/blog/articles/{slug}"));
        Assert.Contains("/blog/articles/" + slug, await visitor.GetStringAsync("/blog/search?q=Wedding"));
        foreach (var path in new[] { "sitemap.xml", "feed.xml", "atom.xml", "feed/json", "llms.txt" })
        {
            var text = await visitor.GetStringAsync("/blog/" + path);
            Assert.Contains("/blog/articles/" + slug, text);
        }
        using var stale = new HttpRequestMessage(HttpMethod.Put, $"/blog/api/articles/{id}")
        {
            Content = JsonContent.Create(new { title = "Stale edit", body = "Old", @abstract = "Old" })
        };
        stale.Headers.TryAddWithoutValidation("If-Match", create.Headers.ETag!.ToString());
        Assert.Equal(HttpStatusCode.PreconditionFailed, (await admin.SendAsync(stale)).StatusCode);
        using var update = new HttpRequestMessage(HttpMethod.Put, $"/blog/api/articles/{id}")
        {
            Content = JsonContent.Create(new { title = "Wedding light updated", body = "New portrait story", @abstract = "Updated sunset story" })
        };
        update.Headers.TryAddWithoutValidation("If-Match", published.Headers.ETag!.ToString());
        var updated = await admin.SendAsync(update);
        Assert.True(updated.IsSuccessStatusCode, await updated.Content.ReadAsStringAsync());
        using var unpublish = new HttpRequestMessage(HttpMethod.Patch, $"/blog/api/articles/{id}/publish")
        {
            Content = JsonContent.Create(new { published = false })
        };
        unpublish.Headers.TryAddWithoutValidation("If-Match", updated.Headers.ETag!.ToString());
        var unpublished = await admin.SendAsync(unpublish);
        Assert.True(unpublished.IsSuccessStatusCode, await unpublished.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.NotFound, (await visitor.GetAsync($"/blog/articles/{slug}")).StatusCode);
        Assert.DoesNotContain("Wedding light", await visitor.GetStringAsync("/blog/feed.xml"));
        using var delete = new HttpRequestMessage(HttpMethod.Delete, $"/blog/api/articles/{id}");
        delete.Headers.TryAddWithoutValidation("If-Match", unpublished.Headers.ETag!.ToString());
        Assert.Equal(HttpStatusCode.NoContent, (await admin.SendAsync(delete)).StatusCode);
    }

    [Fact]
    public async Task Invalid_input_and_missing_antiforgery_are_rejected()
    {
        await using var factory = new StudioFactory();
        using var admin = await factory.Actor();
        Assert.Equal(HttpStatusCode.BadRequest, (await admin.PostAsJsonAsync("/blog/api/articles", new { title = "", body = "", @abstract = "" })).StatusCode);
        admin.DefaultRequestHeaders.Remove("X-XSRF-TOKEN");
        Assert.Equal(HttpStatusCode.BadRequest, (await admin.PostAsJsonAsync("/blog/api/articles", new { title = "Valid", body = "Valid", @abstract = "Valid" })).StatusCode);
    }

    [Fact]
    public async Task Public_listing_pagination_preserves_each_listing_route()
    {
        await using var factory = new StudioFactory();
        using var admin = await factory.Actor();
        using var visitor = await factory.Actor(null);

        for (var index = 0; index < 10; index++)
        {
            var create = await admin.PostAsJsonAsync("/blog/api/articles", new
            {
                title = $"Studio story {index}",
                body = "A published photography story.",
                @abstract = "A short studio note."
            });
            Assert.Equal(HttpStatusCode.Created, create.StatusCode);
            var article = await create.Content.ReadFromJsonAsync<JsonElement>();
            using var publish = new HttpRequestMessage(
                HttpMethod.Patch,
                $"/blog/api/articles/{article.GetProperty("articleId").GetGuid()}/publish")
            {
                Content = JsonContent.Create(new { published = true })
            };
            publish.Headers.TryAddWithoutValidation("If-Match", create.Headers.ETag!.ToString());
            Assert.True((await admin.SendAsync(publish)).IsSuccessStatusCode);
        }

        var home = await visitor.GetStringAsync("/blog");
        Assert.Contains("href=\"/blog?page=2\"", home);
        var articles = await visitor.GetStringAsync("/blog/articles");
        Assert.Contains("href=\"/blog/articles?page=2\"", articles);
        var secondPage = await visitor.GetStringAsync("/blog/articles?page=2");
        Assert.Contains("href=\"/blog/articles?page=1\"", secondPage);
    }

    [Theory]
    [InlineData("/blog/admin/articles")]
    [InlineData("/blog/admin/articles/create")]
    [InlineData("/blog/admin/digital-assets")]
    public async Task Administrator_can_render_imported_editor_pages(string path)
    {
        await using var factory = new StudioFactory();
        using var admin = await factory.Actor();
        var response = await admin.GetAsync(path);
        Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
        Assert.Contains("text/html", response.Content.Headers.ContentType!.ToString());
    }

    // AC-L2-070-07, AC-L2-070-08: the published blog address is /blog, and it renders the approved listing.
    [Fact]
    public async Task Blog_list_page_is_served_at_the_published_blog_address()
    {
        await using var factory = new StudioFactory();
        using var visitor = await factory.Actor(null);
        var response = await visitor.GetAsync("/blog");
        Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("From the studio", html);
        Assert.Contains("<h1>Blog</h1>", html);
        // The canonical link every page already advertises must be the address that renders.
        Assert.Contains("<link rel=\"canonical\" href=\"https://localhost:7443/blog\" />", html);
    }

    // AC-L2-070-07: the trailing-slash form keeps working and settles on the one address.
    [Fact]
    public async Task Trailing_slash_blog_address_redirects_permanently_and_keeps_the_query()
    {
        await using var factory = new StudioFactory();
        using var visitor = await factory.Actor(null);
        var response = await visitor.GetAsync("/blog/?page=2");
        Assert.Equal(HttpStatusCode.MovedPermanently, response.StatusCode);
        Assert.Equal("/blog?page=2", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Client")]
    public async Task Only_studio_administrators_can_read_editor_data(string? role)
    {
        await using var factory = new StudioFactory();
        using var visitor = await factory.Actor(role);
        var response = await visitor.GetAsync("/blog/api/articles");
        Assert.Equal(role == null ? HttpStatusCode.Unauthorized : HttpStatusCode.Forbidden, response.StatusCode);
    }
}
