using MediatR;

namespace QuinntyneBrownStudio.Application.Blog.Articles.Commands;

/// <summary>
/// Publishes the coming-soon article into an empty blog so a gated visitor lands on a page that
/// says what is happening. Returns whether an article was created; a blog that already holds any
/// article, published or not, is left alone.
/// </summary>
public sealed record PublishLaunchArticleCommand : IRequest<bool>;
