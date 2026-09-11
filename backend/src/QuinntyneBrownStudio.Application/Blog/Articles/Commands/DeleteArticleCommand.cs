using QuinntyneBrownStudio.Application.Ports.Blog;
using QuinntyneBrownStudio.Domain.Exceptions.Blog;
using QuinntyneBrownStudio.Application.Blog.Services;
using MediatR;

namespace QuinntyneBrownStudio.Application.Blog.Articles.Commands;

public record DeleteArticleCommand(Guid Id, string? IfMatch) : IRequest;
