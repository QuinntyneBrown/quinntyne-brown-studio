using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Catalog.Promotion;

public sealed class ListPromotionHandler(AdminCatalog catalog) : IRequestHandler<ListPromotion, Qbs.Domain.Entities.Promotion[]>
{
    public Task<Qbs.Domain.Entities.Promotion[]> Handle(ListPromotion request, CancellationToken ct) =>
        catalog.List<Qbs.Domain.Entities.Promotion>();
}
