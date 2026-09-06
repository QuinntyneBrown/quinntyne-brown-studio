using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Catalog.PublicGallery;

public sealed class GetPublicGalleryHandler(AdminCatalog catalog) : IRequestHandler<GetPublicGallery, Qbs.Domain.Entities.PublicGallery>
{
    public async Task<Qbs.Domain.Entities.PublicGallery> Handle(GetPublicGallery request, CancellationToken ct) =>
        await catalog.Get<Qbs.Domain.Entities.PublicGallery>(request.Id) ?? throw new StudioException(404, "Record not found.");
}
