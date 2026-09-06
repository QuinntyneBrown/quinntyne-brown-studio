using MediatR;
using DomainEntities = QuinntyneBrownStudio.Domain.Entities;

namespace QuinntyneBrownStudio.Application.Catalog.Studio;

public sealed class SaveStudioHandler(AdminCatalog catalog)
    : IRequestHandler<SaveStudio, DomainEntities.Studio>
{
    public Task<DomainEntities.Studio> Handle(SaveStudio request, CancellationToken ct) =>
        catalog.Save(request.Value, request.Id);
}
