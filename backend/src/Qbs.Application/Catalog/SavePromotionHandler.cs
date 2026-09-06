using MediatR;
using Qbs.Domain;

namespace Qbs.Application;

public sealed class SavePromotionHandler(AdminCatalog catalog)
    : IRequestHandler<SavePromotion, Promotion>
{
    public Task<Promotion> Handle(SavePromotion request, CancellationToken ct) =>
        catalog.Save(request.Value, request.Id);
}
