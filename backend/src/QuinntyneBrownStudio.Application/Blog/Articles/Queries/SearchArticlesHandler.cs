using QuinntyneBrownStudio.Application.Blog.Models;
using QuinntyneBrownStudio.Application.Blog.Services;
using QuinntyneBrownStudio.Application.Ports.Blog;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace QuinntyneBrownStudio.Application.Blog.Articles.Queries;

public class SearchArticlesHandler(
    IArticleRepository articles,
    ISearchHighlighter highlighter,
    IConfiguration config) : IRequestHandler<SearchArticlesQuery, PagedResponse<SearchResultDto>>
{
    public async Task<PagedResponse<SearchResultDto>> Handle(
        SearchArticlesQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Query) || request.Query.Length > 200 || request.Page < 1 || request.PageSize < 1 || request.PageSize > 100 || request.Page > int.MaxValue / request.PageSize || request.Sort is not ("relevant" or "newest" or "oldest"))
            throw new QuinntyneBrownStudio.Domain.Exceptions.Blog.BadRequestException("Supply a query of 1–200 characters and valid pagination.");
        var (items, total) = await articles.SearchAsync(
            request.Query.Trim(), request.Page, request.PageSize, cancellationToken, request.Sort);

        var baseUrl = (((config["PublicOrigin"] ?? "https://localhost:7443").TrimEnd('/') + "/blog") ?? "").TrimEnd('/');

        var dtos = items.Select(a => new SearchResultDto(
            a.ArticleId,
            a.Title,
            a.Slug,
            Truncate(a.Abstract, 160),
            highlighter.Highlight(a.Title, request.Query),
            highlighter.Highlight(Truncate(a.Abstract, 160), request.Query),
            a.FeaturedImage != null ? $"{baseUrl}/assets/{a.FeaturedImage.StoredFileName}" : null,
            a.DatePublished,
            a.ReadingTimeMinutes)).ToList();

        return new PagedResponse<SearchResultDto>
        {
            Items = dtos,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = total
        };
    }

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s[..max].TrimEnd() + "\u2026";
}
