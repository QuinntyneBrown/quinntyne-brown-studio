using Qbs.Domain;

namespace Qbs.Application;

public sealed class Presentation(IStudioStore store)
{
    public const string Notice = "Subject to change following detailed consultation.";

    public Task<MarketingContent> Save(string key, MarketingContent value) =>
        store.Run(
            "content:" + key,
            async tx =>
            {
                Rules.Require(
                    new[] { "home", "services", "contact" }.Contains(key),
                    "Unknown page key."
                );
                Rules.Text(value.Heading, "heading", 200);
                Rules.Text(value.Body, "body", 10000);
                var prior = (await tx.List<MarketingContent>()).SingleOrDefault(x =>
                    x.PageKey == key
                );
                value.Id = prior?.Id ?? Guid.NewGuid();
                value.PageKey = key;
                value.PublishedHeading = value.Publish ? value.Heading : prior?.PublishedHeading;
                value.PublishedBody = value.Publish ? value.Body : prior?.PublishedBody;
                await tx.Save(value, value.ExpectedVersion);
                return value;
            }
        );

    public Task<object> Public(string kind, string? key = null) =>
        store.Run<object>(
            "presentation",
            async tx =>
                kind switch
                {
                    "promotions" => (await tx.List<Promotion>())
                        .Where(x => x.Published)
                        .Select(x => new
                        {
                            x.Id,
                            x.Title,
                            x.Description,
                            x.IndicativePrice,
                            consultationNotice = Notice,
                        })
                        .ToArray(),
                    "studios" => (await tx.List<Studio>())
                        .Where(x => x.Enabled)
                        .Select(x => new
                        {
                            x.Id,
                            x.Name,
                            x.ResolvedAddress,
                            x.HourlyFee,
                        })
                        .ToArray(),
                    "print-options" => (await tx.List<PrintOption>())
                        .Where(x => x.Enabled)
                        .Select(x => new
                        {
                            x.Id,
                            x.Name,
                            x.Dimensions,
                            x.Finish,
                            x.UnitPrice,
                            revision = x.Version,
                        })
                        .ToArray(),
                    "galleries" => key == null
                        ? (object)
                            (await tx.List<PublicGallery>())
                                .Where(x => x.Published)
                                .Select(GalleryView)
                                .ToArray()
                        : GalleryView(
                            (await tx.List<PublicGallery>()).SingleOrDefault(x =>
                                x.Published && x.Slug == key
                            ) ?? throw new StudioException(404, "Gallery not found.")
                        ),
                    "content" => ContentView(
                        (await tx.List<MarketingContent>()).SingleOrDefault(x =>
                            x.PageKey == key && x.PublishedHeading != null
                        ) ?? throw new StudioException(404, "Content is not published.")
                    ),
                    _ => throw new StudioException(404, "Not found."),
                }
        );

    private static object ContentView(MarketingContent c) =>
        new { heading = c.PublishedHeading, body = c.PublishedBody };

    private static object GalleryView(PublicGallery g) =>
        new
        {
            g.Id,
            g.Title,
            g.Slug,
            photos = g.PhotoIds.Select(id => new
            {
                id,
                name = "Photograph from " + g.Title,
                thumbnailUrl = $"/api/public/galleries/{g.Slug}/photos/{id}?thumbnail=true",
                url = $"/api/public/galleries/{g.Slug}/photos/{id}",
            }),
        };
}
