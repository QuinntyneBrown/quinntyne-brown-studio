using MediatR;
using Qbs.Domain;

namespace Qbs.Application;

public sealed class SavePhotographerHandler(AdminCatalog catalog)
    : IRequestHandler<SavePhotographer, Photographer>
{
    public Task<Photographer> Handle(SavePhotographer request, CancellationToken ct) =>
        catalog.Save(request.Value, request.Id);
}
