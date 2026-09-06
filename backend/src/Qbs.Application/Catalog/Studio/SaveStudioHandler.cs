using MediatR;
using DomainEntities = Qbs.Domain.Entities;

namespace Qbs.Application.Catalog.Studio;

public sealed class SaveStudioHandler(AdminCatalog catalog)
    : IRequestHandler<SaveStudio, DomainEntities.Studio>
{
    public Task<DomainEntities.Studio> Handle(SaveStudio request, CancellationToken ct) =>
        catalog.Save(request.Value, request.Id);
}
