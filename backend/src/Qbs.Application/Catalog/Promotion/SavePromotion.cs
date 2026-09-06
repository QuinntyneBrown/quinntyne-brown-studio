using MediatR;
using DomainEntities = Qbs.Domain.Entities;

namespace Qbs.Application.Catalog.Promotion;

public sealed record SavePromotion(DomainEntities.Promotion Value, Guid? Id)
    : IRequest<DomainEntities.Promotion>;
