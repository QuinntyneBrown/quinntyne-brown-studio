using QuinntyneBrownStudio.Application.Ports.Blog;
using QuinntyneBrownStudio.Domain.Exceptions.Blog;

using MediatR;

namespace QuinntyneBrownStudio.Application.Blog.Articles.Queries;

public record ArticleDto(
    Guid ArticleId, string Title, string Slug, string Abstract,
    string Body, string BodyHtml, Guid? FeaturedImageId, string? FeaturedImageUrl,
    int? FeaturedImageWidth, int? FeaturedImageHeight,
    bool Published, DateTime? DatePublished,
    int ReadingTimeMinutes, DateTime CreatedAt, DateTime UpdatedAt, int Version);
