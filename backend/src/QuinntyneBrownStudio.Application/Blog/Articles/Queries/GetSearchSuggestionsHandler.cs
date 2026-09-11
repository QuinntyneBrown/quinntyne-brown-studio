using QuinntyneBrownStudio.Application.Blog.Services;
using QuinntyneBrownStudio.Application.Ports.Blog;
using MediatR;

namespace QuinntyneBrownStudio.Application.Blog.Articles.Queries;

public class GetSearchSuggestionsHandler(
    IArticleRepository articles,
    ISearchHighlighter highlighter)
    : IRequestHandler<GetSearchSuggestionsQuery, IReadOnlyList<SearchSuggestionDto>>
{
    public async Task<IReadOnlyList<SearchSuggestionDto>> Handle(
        GetSearchSuggestionsQuery request, CancellationToken cancellationToken)
    {
        if (request.Query?.Length > 200) throw new QuinntyneBrownStudio.Domain.Exceptions.Blog.BadRequestException("Query must not exceed 200 characters.");
        if (string.IsNullOrWhiteSpace(request.Query) || request.Query.Length < 2) return [];
        var items = await articles.GetSuggestionsAsync(request.Query, cancellationToken);
        return items.Select(a => new SearchSuggestionDto(
            a.Title,
            a.Slug,
            highlighter.Highlight(a.Title, request.Query))).ToList();
    }
}
