using MediatR;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Domain.Entities;

namespace QuinntyneBrownStudio.Application.Presentation;

public sealed class GetContactPageHandler(IStudioStore store) : IRequestHandler<GetContactPage, ContactPageView>
{
    public Task<ContactPageView> Handle(GetContactPage request, CancellationToken ct) =>
        store.Run(
            "presentation",
            async tx =>
            {
                var content = (await tx.List<MarketingContent>()).SingleOrDefault(x => x.PageKey == "contact");
                var published = content?.PublishedHeading != null;
                var details = await tx.Get<StudioDetails>(AdminCatalog.ConfigurationId);
                var configured =
                    details != null
                    && (details.Email ?? details.Phone ?? details.Hours ?? details.ReplyNote) != null;
                var studios = await tx.List<Studio>();
                var enabled = studios
                    .Where(x => x.Enabled)
                    .OrderByDescending(x => x.IsBase)
                    .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                    .ToArray();
                var discounts = await tx.Get<DiscountConfiguration>(AdminCatalog.ConfigurationId);
                var advance =
                    discounts?.AdvanceRule is { Enabled: true } rule
                        ? new ContactAdvanceBooking(rule.Threshold, rule.Percentage)
                        : null;
                var fingerprint = string.Join(
                    ",",
                    new Entity?[] { content, details, discounts }
                        .Concat(studios)
                        .Where(x => x != null)
                        .Select(x => $"{x!.Id}:{x.Version}")
                );
                return new ContactPageView(
                    published ? content!.PublishedHeading! : ContactPageCopy.DefaultHeading,
                    published ? content!.PublishedBody ?? "" : ContactPageCopy.DefaultIntroduction,
                    ContactPageCopy.ReplyNotice(configured ? details!.ReplyNote : null),
                    configured ? details : null,
                    enabled.Select(x => new ContactStudio(x.Name, x.ResolvedAddress.Label, x.IsBase)).ToArray(),
                    advance,
                    ContactPageCopy.Questions(advance),
                    fingerprint
                );
            },
            ct
        );
}
