using QuinntyneBrownStudio.Application.Blog.Models;
using QuinntyneBrownStudio.Application.Blog.Services;
using QuinntyneBrownStudio.Application.Ports.Blog;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace QuinntyneBrownStudio.Application.Blog.Articles.Queries;

public record SearchArticlesQuery(string Query, int Page = 1, int PageSize = 10, string Sort = "relevant")
    : IRequest<PagedResponse<SearchResultDto>>;
