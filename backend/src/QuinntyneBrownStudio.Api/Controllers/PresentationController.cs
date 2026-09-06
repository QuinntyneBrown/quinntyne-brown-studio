using MediatR;
using QuinntyneBrownStudio.Application.Presentation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using PresentationService = QuinntyneBrownStudio.Application.Presentation.Presentation;

namespace QuinntyneBrownStudio.Api.Controllers;

[ApiController]
public sealed class PresentationController(ISender sender)
    : ControllerBase
{
    [HttpGet("api/public/promotions")]
    public Task<object> Promotions() => sender.Send(new GetPublishedPresentation("promotions"));

    [HttpGet("api/public/print-options")]
    public Task<object> Prints() => sender.Send(new GetPublishedPresentation("print-options"));

    [HttpGet("api/public/galleries")]
    public Task<object> Galleries() => sender.Send(new GetPublishedPresentation("galleries"));

    [HttpGet("api/public/galleries/{slug}")]
    public Task<object> Gallery(string slug) => sender.Send(new GetPublishedPresentation("galleries", slug));

    [HttpGet("api/public/content/{key}")]
    public Task<object> GetContent(string key) => sender.Send(new GetPublishedPresentation("content", key));

    [Authorize(Roles = "Administrator"), HttpGet("api/admin/content")]
    public Task<MarketingContent[]> Contents() => sender.Send(new ListMarketingContent());

    [Authorize(Roles = "Administrator"), HttpPut("api/admin/content/{key}")]
    public Task<MarketingContent> Save(string key, MarketingContent value) =>
        sender.Send(new SaveMarketingContent(key, value));
}
