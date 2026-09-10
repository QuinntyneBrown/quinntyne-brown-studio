using QuinntyneBrownStudio.Application.Blog.Models;
using QuinntyneBrownStudio.Application.Blog.Articles.Commands;
using QuinntyneBrownStudio.Application.Blog.Articles.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace QuinntyneBrownStudio.Api.Models;

public record PublishArticleBody(bool Published);
