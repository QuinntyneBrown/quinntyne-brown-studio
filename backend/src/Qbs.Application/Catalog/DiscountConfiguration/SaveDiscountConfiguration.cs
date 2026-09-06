using MediatR;
using DomainEntities = Qbs.Domain.Entities;

namespace Qbs.Application.Catalog.DiscountConfiguration;

public sealed record SaveDiscountConfiguration(DomainEntities.DiscountConfiguration Value, Guid? Id)
    : IRequest<DomainEntities.DiscountConfiguration>;
