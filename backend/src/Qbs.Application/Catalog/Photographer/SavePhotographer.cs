using MediatR;
using DomainEntities = Qbs.Domain.Entities;

namespace Qbs.Application.Catalog.Photographer;

public sealed record SavePhotographer(DomainEntities.Photographer Value, Guid? Id)
    : IRequest<DomainEntities.Photographer>;
