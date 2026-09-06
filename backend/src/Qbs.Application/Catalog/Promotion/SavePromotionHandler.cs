using MediatR;
using DomainEntities = Qbs.Domain.Entities;

namespace Qbs.Application.Catalog.Promotion;

public sealed class SavePromotionHandler(AdminCatalog catalog)
    : IRequestHandler<SavePromotion, DomainEntities.Promotion>
{
    public Task<DomainEntities.Promotion> Handle(SavePromotion request, CancellationToken ct) =>
        catalog.Save(request.Value, request.Id);
}
