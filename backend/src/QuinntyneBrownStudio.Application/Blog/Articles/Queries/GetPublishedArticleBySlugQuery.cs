using QuinntyneBrownStudio.Application.Ports.Blog;
using QuinntyneBrownStudio.Domain.Exceptions.Blog;

using MediatR;

namespace QuinntyneBrownStudio.Application.Blog.Articles.Queries;

public record GetPublishedArticleBySlugQuery(string Slug) : IRequest<ArticleDto>;
