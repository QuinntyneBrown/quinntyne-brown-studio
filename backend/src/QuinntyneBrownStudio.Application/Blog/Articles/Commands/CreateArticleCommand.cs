using QuinntyneBrownStudio.Domain.Exceptions.Blog;
using QuinntyneBrownStudio.Domain.Entities.Blog;
using QuinntyneBrownStudio.Application.Ports.Blog;
using QuinntyneBrownStudio.Application.Blog.Services;
using FluentValidation;
using MediatR;
using QuinntyneBrownStudio.Application.Blog.Articles.Queries;

namespace QuinntyneBrownStudio.Application.Blog.Articles.Commands;

public record CreateArticleCommand(string Title, string Body, string Abstract, Guid? FeaturedImageId) : IRequest<ArticleDto>;
