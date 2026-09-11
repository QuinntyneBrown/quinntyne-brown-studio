using QuinntyneBrownStudio.Application.Blog.Services;
using QuinntyneBrownStudio.Application.Ports.Blog;
using MediatR;

namespace QuinntyneBrownStudio.Application.Blog.Articles.Queries;

public record GetSearchSuggestionsQuery(string Query)
    : IRequest<IReadOnlyList<SearchSuggestionDto>>;
