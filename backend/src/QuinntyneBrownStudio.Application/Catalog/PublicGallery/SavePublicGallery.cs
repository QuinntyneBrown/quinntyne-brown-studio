using MediatR;
using DomainEntities = QuinntyneBrownStudio.Domain.Entities;

namespace QuinntyneBrownStudio.Application.Catalog.PublicGallery;

public sealed record SavePublicGallery(DomainEntities.PublicGallery Value, Guid? Id)
    : IRequest<DomainEntities.PublicGallery>;
