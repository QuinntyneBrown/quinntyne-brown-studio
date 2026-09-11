using QuinntyneBrownStudio.Application.Ports.Blog;
using QuinntyneBrownStudio.Application.Blog.Models;

using MediatR;

namespace QuinntyneBrownStudio.Application.Blog.Articles.Queries;

public class GetPublishedArticlesHandler(IArticleRepository articles) : IRequestHandler<GetPublishedArticlesQuery, PagedResponse<ArticleListDto>>
{
    public async Task<PagedResponse<ArticleListDto>> Handle(GetPublishedArticlesQuery request, CancellationToken cancellationToken)
    {
        if (request.Page < 1 || request.PageSize < 1 || request.PageSize > 100 || request.Page > int.MaxValue / request.PageSize)
            throw new QuinntyneBrownStudio.Domain.Exceptions.Blog.BadRequestException("Page must be positive and page size between 1 and 100.");
        var items = await articles.GetPublishedAsync(request.Page, request.PageSize, cancellationToken);
        var total = await articles.GetPublishedCountAsync(cancellationToken);
        return new PagedResponse<ArticleListDto>
        {
            Items = items.Select(a => new ArticleListDto(
                a.ArticleId, a.Title, a.Slug, a.Abstract,
                a.FeaturedImageId, a.FeaturedImage != null ? $"/blog/assets/{a.FeaturedImage.StoredFileName}" : null,
                a.FeaturedImage?.Width > 0 ? a.FeaturedImage.Width : (int?)null,
                a.FeaturedImage?.Height > 0 ? a.FeaturedImage.Height : (int?)null,
                a.Published, a.DatePublished,
                a.ReadingTimeMinutes, a.CreatedAt, a.UpdatedAt, a.Version)).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = total
        };
    }
}
