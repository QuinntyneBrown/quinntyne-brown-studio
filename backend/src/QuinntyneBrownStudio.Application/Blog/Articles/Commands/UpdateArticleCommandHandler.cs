using QuinntyneBrownStudio.Application.Ports.Blog;
using QuinntyneBrownStudio.Domain.Exceptions.Blog;
using QuinntyneBrownStudio.Application.Blog.Services;
using FluentValidation;
using MediatR;
using QuinntyneBrownStudio.Application.Blog.Articles.Queries;

namespace QuinntyneBrownStudio.Application.Blog.Articles.Commands;

public class UpdateArticleCommandHandler(
    IUnitOfWork uow,
    ISlugGenerator slugGenerator,
    IMarkdownConverter markdownConverter,
    IReadingTimeCalculator readingTimeCalculator) : IRequestHandler<UpdateArticleCommand, ArticleDto>
{
    public async Task<ArticleDto> Handle(UpdateArticleCommand request, CancellationToken cancellationToken)
    {
        if (request.FeaturedImageId is { } imageId && await uow.DigitalAssets.GetByIdAsync(imageId, cancellationToken) == null)
            throw new BadRequestException("The selected featured image does not exist.");

        var article = await uow.Articles.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Article with ID '{request.Id}' was not found.");

        var expectedETag = $"W/\"article-{article.ArticleId}-v{article.Version}\"";
        if (!string.IsNullOrEmpty(request.IfMatch) && request.IfMatch != expectedETag)
            throw new PreconditionFailedException("The article has been modified. Please refresh and try again.");

        if (article.Title != request.Title && !article.Published)
        {
            var slug = slugGenerator.Generate(request.Title);
            if (await uow.Articles.SlugExistsAsync(slug, article.ArticleId, cancellationToken))
                throw new ConflictException($"An article with slug '{slug}' already exists.");
            article.Slug = slug;
        }

        if (article.Body != request.Body)
        {
            article.BodyHtml = markdownConverter.Convert(request.Body);
            article.ReadingTimeMinutes = readingTimeCalculator.Calculate(request.Body);
        }

        article.Title = request.Title;
        article.Body = request.Body;
        article.Abstract = request.Abstract;
        article.FeaturedImageId = request.FeaturedImageId;
        article.UpdatedAt = DateTime.UtcNow;

        uow.Articles.Update(article);
        await uow.SaveChangesAsync(cancellationToken);


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
