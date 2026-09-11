using QuinntyneBrownStudio.Application.Ports.Blog;
using QuinntyneBrownStudio.Application.Blog.Models;

using MediatR;

namespace QuinntyneBrownStudio.Application.Blog.Articles.Queries;

public record ArticleListDto(
    Guid ArticleId, string Title, string Slug, string Abstract,
    Guid? FeaturedImageId, string? FeaturedImageUrl, int? FeaturedImageWidth, int? FeaturedImageHeight,
    bool Published, DateTime? DatePublished,
    int ReadingTimeMinutes, DateTime CreatedAt, DateTime UpdatedAt, int Version);
