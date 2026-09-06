using MediatR;
using Qbs.Domain;

namespace Qbs.Application;

public sealed class SavePublicGalleryHandler(AdminCatalog catalog)
    : IRequestHandler<SavePublicGallery, PublicGallery>
{
    public Task<PublicGallery> Handle(SavePublicGallery request, CancellationToken ct) =>
        catalog.Save(request.Value, request.Id);
}
