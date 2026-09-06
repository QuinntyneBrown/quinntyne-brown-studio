using MediatR;
using DomainEntities = QuinntyneBrownStudio.Domain.Entities;

namespace QuinntyneBrownStudio.Application.Catalog.Photographer;

public sealed record SavePhotographer(DomainEntities.Photographer Value, Guid? Id)
    : IRequest<DomainEntities.Photographer>;
