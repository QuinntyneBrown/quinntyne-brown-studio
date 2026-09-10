using QuinntyneBrownStudio.Application.Blog.Models;
using QuinntyneBrownStudio.Application.Blog.Services;
using QuinntyneBrownStudio.Application.Ports.Blog;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace QuinntyneBrownStudio.Application.Blog.Articles.Queries;

public record SearchResultDto(
    Guid ArticleId,
    string Title,
    string Slug,
    string Abstract,
    string TitleHighlighted,
    string AbstractHighlighted,
    string? FeaturedImageUrl,
    DateTime? DatePublished,
    int ReadingTimeMinutes);
