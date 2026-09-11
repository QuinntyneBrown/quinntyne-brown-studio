using QuinntyneBrownStudio.Application.Ports.Blog;
using QuinntyneBrownStudio.Application.Blog.Models;

using MediatR;

namespace QuinntyneBrownStudio.Application.Blog.Articles.Queries;

public record GetPublishedArticlesQuery(int Page = 1, int PageSize = 9) : IRequest<PagedResponse<ArticleListDto>>;
