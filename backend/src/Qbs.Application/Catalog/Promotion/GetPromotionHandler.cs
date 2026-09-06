using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Catalog.Promotion;

public sealed class GetPromotionHandler(AdminCatalog catalog) : IRequestHandler<GetPromotion, Qbs.Domain.Entities.Promotion>
{
    public async Task<Qbs.Domain.Entities.Promotion> Handle(GetPromotion request, CancellationToken ct) =>
        await catalog.Get<Qbs.Domain.Entities.Promotion>(request.Id) ?? throw new StudioException(404, "Record not found.");
}
