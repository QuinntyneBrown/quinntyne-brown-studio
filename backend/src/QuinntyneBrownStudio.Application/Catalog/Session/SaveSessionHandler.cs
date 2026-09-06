using MediatR;
using DomainEntities = QuinntyneBrownStudio.Domain.Entities;

namespace QuinntyneBrownStudio.Application.Catalog.Session;

public sealed class SaveSessionHandler(AdminCatalog catalog)
    : IRequestHandler<SaveSession, DomainEntities.Session>
{
    public Task<DomainEntities.Session> Handle(SaveSession request, CancellationToken ct) =>
        catalog.Save(request.Value, request.Id);
}
