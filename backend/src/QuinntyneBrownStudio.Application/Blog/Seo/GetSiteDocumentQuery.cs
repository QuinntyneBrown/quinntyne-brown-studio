using MediatR;

namespace QuinntyneBrownStudio.Application.Blog.Seo;

/// <summary>The site's root discovery documents: <c>robots</c> or <c>sitemap</c>.</summary>
public sealed record GetSiteDocumentQuery(string Format) : IRequest<SiteDocument>;
