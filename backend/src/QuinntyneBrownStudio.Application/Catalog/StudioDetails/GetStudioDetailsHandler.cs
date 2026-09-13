using MediatR;
using DomainEntities = QuinntyneBrownStudio.Domain.Entities;

namespace QuinntyneBrownStudio.Application.Catalog.StudioDetails;

public sealed class GetStudioDetailsHandler(AdminCatalog catalog)
    : IRequestHandler<GetStudioDetails, DomainEntities.StudioDetails>
{
    public async Task<DomainEntities.StudioDetails> Handle(GetStudioDetails request, CancellationToken ct) =>
        await catalog.Get<DomainEntities.StudioDetails>(AdminCatalog.ConfigurationId)
        ?? new DomainEntities.StudioDetails { Id = AdminCatalog.ConfigurationId };
}
