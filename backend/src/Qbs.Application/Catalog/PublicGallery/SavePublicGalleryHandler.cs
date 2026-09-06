using MediatR;
using DomainEntities = Qbs.Domain.Entities;

namespace Qbs.Application.Catalog.PublicGallery;

public sealed class SavePublicGalleryHandler(AdminCatalog catalog)
    : IRequestHandler<SavePublicGallery, DomainEntities.PublicGallery>
{
    public Task<DomainEntities.PublicGallery> Handle(SavePublicGallery request, CancellationToken ct) =>
        catalog.Save(request.Value, request.Id);
}
