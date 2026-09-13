using System.Xml.Linq;
using MediatR;
using Microsoft.Extensions.Configuration;
using QuinntyneBrownStudio.Application.Blog.Articles.Queries;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Domain.Entities;

namespace QuinntyneBrownStudio.Application.Blog.Seo;

/// <summary>
/// Builds the root robots directives and the one site sitemap: the marketing home, /about, /contact
/// (each dated by its published content), the blog listing, and every published article.
/// </summary>
public sealed class GetSiteDocumentHandler(IStudioStore store, IMediator mediator, IConfiguration configuration)
    : IRequestHandler<GetSiteDocumentQuery, SiteDocument>
{
    private static readonly XNamespace Sitemap = "http://www.sitemaps.org/schemas/sitemap/0.9";

    public Task<SiteDocument> Handle(GetSiteDocumentQuery request, CancellationToken ct) =>
        request.Format switch
        {
            "robots" => Task.FromResult(Robots()),
            "sitemap" => SiteMap(ct),
            _ => throw new ArgumentOutOfRangeException(nameof(request)),
        };

    private string Origin => (configuration["PublicOrigin"] ?? "https://localhost:7443").TrimEnd('/');

    private SiteDocument Robots() =>
        new(
            "User-agent: *\nAllow: /\nDisallow: /admin/\nDisallow: /client/\nDisallow: /api/\nDisallow: /blog/admin/\nDisallow: /blog/api/\nSitemap: "
                + Origin
                + "/sitemap.xml\n",
            "text/plain"
        );

    private async Task<SiteDocument> SiteMap(CancellationToken ct)
    {
        var content = await store.Run("presentation", tx => tx.List<MarketingContent>(), ct);
        DateTimeOffset? Published(string key) =>
            content.SingleOrDefault(x => x.PageKey == key && x.PublishedHeading != null)?.PublishedAt;
        var urls = new List<XElement>
        {
            Url("/", Published("home"), "weekly", "1.0"),
            Url("/about", Published("about"), "monthly", "0.8"),
            Url("/contact", Published("contact"), "monthly", "0.8"),
            Url("/blog", null, "daily", "0.9"),
        };
        var articles = await mediator.Send(new GetPublishedArticlesQuery(1, 100), ct);
        for (var page = 2; page <= articles.TotalPages; page++)
            articles.Items.AddRange((await mediator.Send(new GetPublishedArticlesQuery(page, 100), ct)).Items);
        urls.AddRange(articles.Items.Select(article =>
            Url($"/blog/articles/{article.Slug}", article.UpdatedAt, "weekly", "0.7")));
        var document = new XDocument(new XDeclaration("1.0", "utf-8", null), new XElement(Sitemap + "urlset", urls));
        return new SiteDocument(document.ToString(), "application/xml; charset=utf-8");
    }

    private XElement Url(string path, DateTimeOffset? modified, string frequency, string priority)
    {
        var url = new XElement(Sitemap + "url", new XElement(Sitemap + "loc", Origin + path));
        if (modified != null)
            url.Add(new XElement(Sitemap + "lastmod", modified.Value.ToUniversalTime().ToString("yyyy-MM-dd")));
        url.Add(new XElement(Sitemap + "changefreq", frequency), new XElement(Sitemap + "priority", priority));
        return url;
    }
}
