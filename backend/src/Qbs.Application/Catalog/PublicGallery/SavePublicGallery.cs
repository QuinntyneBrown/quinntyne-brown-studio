using MediatR;
using DomainEntities = Qbs.Domain.Entities;

namespace Qbs.Application.Catalog.PublicGallery;

public sealed record SavePublicGallery(DomainEntities.PublicGallery Value, Guid? Id)
    : IRequest<DomainEntities.PublicGallery>;
