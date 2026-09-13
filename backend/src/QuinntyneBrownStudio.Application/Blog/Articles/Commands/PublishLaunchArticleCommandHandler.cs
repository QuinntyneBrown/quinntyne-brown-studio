using MediatR;
using QuinntyneBrownStudio.Application.Blog.Services;
using QuinntyneBrownStudio.Application.Ports.Blog;
using QuinntyneBrownStudio.Domain.Entities.Blog;

namespace QuinntyneBrownStudio.Application.Blog.Articles.Commands;

public sealed class PublishLaunchArticleCommandHandler(
    IUnitOfWork uow,
    ISlugGenerator slugGenerator,
    IMarkdownConverter markdownConverter,
    IReadingTimeCalculator readingTimeCalculator) : IRequestHandler<PublishLaunchArticleCommand, bool>
{
    public async Task<bool> Handle(PublishLaunchArticleCommand request, CancellationToken cancellationToken)
    {
        if (await uow.Articles.GetAllCountAsync(cancellationToken) > 0)
            return false;
        var now = DateTime.UtcNow;
        await uow.Articles.AddAsync(new Article
        {
            ArticleId = Guid.NewGuid(),
            Title = LaunchArticle.Title,
            Slug = slugGenerator.Generate(LaunchArticle.Title),
            Abstract = LaunchArticle.Abstract,
            Body = LaunchArticle.Body,
            BodyHtml = markdownConverter.Convert(LaunchArticle.Body),
            ReadingTimeMinutes = readingTimeCalculator.Calculate(LaunchArticle.Body),
            Published = true,
            DatePublished = now,
            Version = 1,
            CreatedAt = now,
            UpdatedAt = now,
        }, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}
