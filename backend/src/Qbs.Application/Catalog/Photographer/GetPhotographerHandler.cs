using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Catalog.Photographer;

public sealed class GetPhotographerHandler(AdminCatalog catalog) : IRequestHandler<GetPhotographer, Qbs.Domain.Entities.Photographer>
{
    public async Task<Qbs.Domain.Entities.Photographer> Handle(GetPhotographer request, CancellationToken ct) =>
        await catalog.Get<Qbs.Domain.Entities.Photographer>(request.Id) ?? throw new StudioException(404, "Record not found.");
}
