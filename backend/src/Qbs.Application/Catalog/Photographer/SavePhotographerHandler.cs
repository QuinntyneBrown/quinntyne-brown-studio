using MediatR;
using DomainEntities = Qbs.Domain.Entities;

namespace Qbs.Application.Catalog.Photographer;

public sealed class SavePhotographerHandler(AdminCatalog catalog)
    : IRequestHandler<SavePhotographer, DomainEntities.Photographer>
{
    public Task<DomainEntities.Photographer> Handle(SavePhotographer request, CancellationToken ct) =>
        catalog.Save(request.Value, request.Id);
}
