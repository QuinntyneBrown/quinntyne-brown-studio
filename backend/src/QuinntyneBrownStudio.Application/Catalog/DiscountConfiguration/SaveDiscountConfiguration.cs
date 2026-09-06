using MediatR;
using DomainEntities = QuinntyneBrownStudio.Domain.Entities;

namespace QuinntyneBrownStudio.Application.Catalog.DiscountConfiguration;

public sealed record SaveDiscountConfiguration(DomainEntities.DiscountConfiguration Value, Guid? Id)
    : IRequest<DomainEntities.DiscountConfiguration>;
