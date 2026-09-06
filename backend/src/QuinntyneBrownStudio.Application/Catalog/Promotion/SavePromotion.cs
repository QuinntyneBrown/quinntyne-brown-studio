using MediatR;
using DomainEntities = QuinntyneBrownStudio.Domain.Entities;

namespace QuinntyneBrownStudio.Application.Catalog.Promotion;

public sealed record SavePromotion(DomainEntities.Promotion Value, Guid? Id)
    : IRequest<DomainEntities.Promotion>;
