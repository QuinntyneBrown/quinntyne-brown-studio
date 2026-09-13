using MediatR;
using DomainEntities = QuinntyneBrownStudio.Domain.Entities;

namespace QuinntyneBrownStudio.Application.Catalog.StudioDetails;

public sealed record SaveStudioDetails(DomainEntities.StudioDetails Value, Guid? Id)
    : IRequest<DomainEntities.StudioDetails>;
