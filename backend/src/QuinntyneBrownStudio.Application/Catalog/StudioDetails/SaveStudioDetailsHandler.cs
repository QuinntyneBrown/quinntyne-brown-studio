using MediatR;
using DomainEntities = QuinntyneBrownStudio.Domain.Entities;

namespace QuinntyneBrownStudio.Application.Catalog.StudioDetails;

public sealed class SaveStudioDetailsHandler(AdminCatalog catalog)
    : IRequestHandler<SaveStudioDetails, DomainEntities.StudioDetails>
{
    public Task<DomainEntities.StudioDetails> Handle(SaveStudioDetails request, CancellationToken ct) =>
        catalog.Save(request.Value, request.Id);
}
