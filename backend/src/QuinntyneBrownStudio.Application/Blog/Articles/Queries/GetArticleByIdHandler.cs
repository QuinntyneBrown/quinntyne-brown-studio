using QuinntyneBrownStudio.Application.Ports.Blog;
using QuinntyneBrownStudio.Domain.Exceptions.Blog;

using MediatR;

namespace QuinntyneBrownStudio.Application.Blog.Articles.Queries;

public class GetArticleByIdHandler(IArticleRepository articles) : IRequestHandler<GetArticleByIdQuery, ArticleDto>
{
    public async Task<ArticleDto> Handle(GetArticleByIdQuery request, CancellationToken cancellationToken)
    {
        var article = await articles.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Article with ID '{request.Id}' was not found.");

        return new ArticleDto(
            article.ArticleId, article.Title, article.Slug, article.Abstract,
            article.Body, article.BodyHtml, article.FeaturedImageId,
            article.FeaturedImage != null ? $"/blog/assets/{article.FeaturedImage.StoredFileName}" : null,
            article.FeaturedImage?.Width > 0 ? article.FeaturedImage.Width : (int?)null,
            article.FeaturedImage?.Height > 0 ? article.FeaturedImage.Height : (int?)null,
            article.Published, article.DatePublished,
            article.ReadingTimeMinutes, article.CreatedAt, article.UpdatedAt, article.Version);
    }
}
