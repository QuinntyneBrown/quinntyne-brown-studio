using MediatR;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuinntyneBrownStudio.Application.Clients;
using QuinntyneBrownStudio.Domain.Entities;

namespace QuinntyneBrownStudio.Api.Controllers;

[ApiController, Authorize(Roles = "Client"), Route("api/client/albums")]
public sealed class AlbumsController(ISender sender) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public Task<object> List() => sender.Send(new GetClientAlbums(UserId));

    [HttpGet("{id:guid}")]
    public Task<object> Get(Guid id) => sender.Send(new GetClientAlbums(UserId, id));

    [HttpPost]
    public async Task<IActionResult> Create(Album input)
    {
        var album = await sender.Send(new SaveClientAlbum(UserId, null, input));
        return Created($"/api/client/albums/{album.Id}", album);
    }

    [HttpPut("{id:guid}")]
    public Task<Album> Save(Guid id, Album input) => sender.Send(new SaveClientAlbum(UserId, id, input));
}
