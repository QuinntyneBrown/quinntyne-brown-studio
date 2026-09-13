using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Domain.Entities;

namespace QuinntyneBrownStudio.Application.Presentation;

public sealed class GetAboutPageHandler(IStudioStore store) : IRequestHandler<GetAboutPage, AboutPageView>
{
    public Task<AboutPageView> Handle(GetAboutPage request, CancellationToken ct) =>
        store.Run(
            "presentation",
            async tx =>
            {
                var content = (await tx.List<MarketingContent>()).SingleOrDefault(x => x.PageKey == "about");
                var published = content?.PublishedHeading != null;
                var photographers = await tx.List<Photographer>();
                var team = photographers.Where(x => x.Active).OrderBy(x => x.CreatedAt).ToArray();
                var galleries = (await tx.List<PublicGallery>()).Where(x => x.Published).ToArray();
                var hero = galleries
                    .Where(x => x.PhotoIds.Length > 0)
                    .OrderByDescending(x => x.PublishedAt ?? DateTimeOffset.MinValue)
                    .FirstOrDefault();
                var fingerprint = string.Join(
                    ",",
                    new Entity?[] { content }
                        .Concat(photographers)
                        .Concat(galleries)
                        .Where(x => x != null)
                        .Select(x => $"{x!.Id}:{x.Version}")
                );
                return new AboutPageView(
                    published ? content!.PublishedHeading! : AboutPageCopy.DefaultHeading,
                    published ? content!.PublishedBody ?? "" : AboutPageCopy.DefaultIntroduction,
                    !published,
                    team.Select((p, index) => new AboutTeamMember(
                            p.Name,
                            index == 0 ? AboutPageCopy.FounderRole : AboutPageCopy.PhotographerRole
                        ))
                        .ToArray(),
                    hero == null
                        ? null
                        : new AboutHeroPhoto(hero.Title, $"/api/public/galleries/{hero.Slug}/photos/{hero.PhotoIds[0]}"),
                    fingerprint
                );
            },
            ct
        );
}
