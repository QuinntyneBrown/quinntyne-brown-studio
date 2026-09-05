using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qbs.Application;
using Qbs.Domain;

namespace Qbs.Api.Controllers;

[ApiController, Authorize(Roles = "Client"), Route("api/client/albums")]
public sealed class AlbumsController(ClientWorkflows clients) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public Task<object> List() => clients.Albums(UserId);

    [HttpGet("{id:guid}")]
    public Task<object> Get(Guid id) => clients.Albums(UserId, id);

    [HttpPost]
    public async Task<IActionResult> Create(Album input)
    {
        var album = await clients.SaveAlbum(UserId, null, input);
        return Created($"/api/client/albums/{album.Id}", album);
    }

    [HttpPut("{id:guid}")]
    public Task<Album> Save(Guid id, Album input) => clients.SaveAlbum(UserId, id, input);
}
