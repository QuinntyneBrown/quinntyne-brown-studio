using MediatR;
using DomainEntities = QuinntyneBrownStudio.Domain.Entities;

namespace QuinntyneBrownStudio.Application.Catalog.StudioDetails;

public sealed record GetStudioDetails : IRequest<DomainEntities.StudioDetails>;
