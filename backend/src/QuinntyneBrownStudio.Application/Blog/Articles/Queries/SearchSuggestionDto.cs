using QuinntyneBrownStudio.Application.Blog.Services;
using QuinntyneBrownStudio.Application.Ports.Blog;
using MediatR;

namespace QuinntyneBrownStudio.Application.Blog.Articles.Queries;

public record SearchSuggestionDto(
    string Title,
    string Slug,
    string TitleHighlighted);
