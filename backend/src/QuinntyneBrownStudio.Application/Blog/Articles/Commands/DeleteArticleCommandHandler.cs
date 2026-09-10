using QuinntyneBrownStudio.Application.Ports.Blog;
using QuinntyneBrownStudio.Domain.Exceptions.Blog;
using QuinntyneBrownStudio.Application.Blog.Services;
using MediatR;

namespace QuinntyneBrownStudio.Application.Blog.Articles.Commands;

public class DeleteArticleCommandHandler(IUnitOfWork uow) : IRequestHandler<DeleteArticleCommand>
{
    public async Task Handle(DeleteArticleCommand request, CancellationToken cancellationToken)
    {
        var article = await uow.Articles.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Article with ID '{request.Id}' was not found.");

        var expectedETag = $"W/\"article-{article.ArticleId}-v{article.Version}\"";
        if (!string.IsNullOrEmpty(request.IfMatch) && request.IfMatch != expectedETag)
            throw new PreconditionFailedException("The article has been modified. Please refresh and try again.");

        var slug = article.Slug;
        article.FeaturedImageId = null;
        uow.Articles.Remove(article);
        await uow.SaveChangesAsync(cancellationToken);

    }
}
