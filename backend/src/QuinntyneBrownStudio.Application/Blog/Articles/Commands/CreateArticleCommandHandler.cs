using QuinntyneBrownStudio.Domain.Exceptions.Blog;
using QuinntyneBrownStudio.Domain.Entities.Blog;
using QuinntyneBrownStudio.Application.Ports.Blog;
using QuinntyneBrownStudio.Application.Blog.Services;
using FluentValidation;
using MediatR;
using QuinntyneBrownStudio.Application.Blog.Articles.Queries;

namespace QuinntyneBrownStudio.Application.Blog.Articles.Commands;

public class CreateArticleCommandHandler(
    IUnitOfWork uow,
    ISlugGenerator slugGenerator,
    IMarkdownConverter markdownConverter,
    IReadingTimeCalculator readingTimeCalculator) : IRequestHandler<CreateArticleCommand, ArticleDto>
{
    public async Task<ArticleDto> Handle(CreateArticleCommand request, CancellationToken cancellationToken)
    {
        if (request.FeaturedImageId is { } imageId && await uow.DigitalAssets.GetByIdAsync(imageId, cancellationToken) == null)
            throw new BadRequestException("The selected featured image does not exist.");

        var slug = slugGenerator.Generate(request.Title);

        if (await uow.Articles.SlugExistsAsync(slug, cancellationToken: cancellationToken))
            throw new ConflictException($"An article with slug '{slug}' already exists.");

        var bodyHtml = markdownConverter.Convert(request.Body);
        var readingTime = readingTimeCalculator.Calculate(request.Body);

        var article = new Article
        {
            ArticleId = Guid.NewGuid(),
            Title = request.Title,
            Slug = slug,
            Abstract = request.Abstract,
            Body = request.Body,
            BodyHtml = bodyHtml,
            FeaturedImageId = request.FeaturedImageId,
            Published = false,
            ReadingTimeMinutes = readingTime,
            Version = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await uow.Articles.AddAsync(article, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);

        return new ArticleDto(
            article.ArticleId, article.Title, article.Slug, article.Abstract,
            article.Body, article.BodyHtml, article.FeaturedImageId, null,
            null, null,
            article.Published, article.DatePublished,
            article.ReadingTimeMinutes, article.CreatedAt, article.UpdatedAt, article.Version);
    }
}
