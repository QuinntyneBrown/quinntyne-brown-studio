using QuinntyneBrownStudio.Domain.Exceptions.Blog;
using QuinntyneBrownStudio.Domain.Entities.Blog;
using QuinntyneBrownStudio.Application.Ports.Blog;
using QuinntyneBrownStudio.Application.Blog.Services;
using FluentValidation;
using MediatR;
using QuinntyneBrownStudio.Application.Blog.Articles.Queries;

namespace QuinntyneBrownStudio.Application.Blog.Articles.Commands;

public class CreateArticleCommandValidator : AbstractValidator<CreateArticleCommand>
{
    public CreateArticleCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Body).NotEmpty();
        RuleFor(x => x.Abstract).NotEmpty().MaximumLength(512);
    }
}
