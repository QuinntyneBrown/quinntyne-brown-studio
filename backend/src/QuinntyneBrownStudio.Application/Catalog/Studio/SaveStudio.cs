using MediatR;
using DomainEntities = QuinntyneBrownStudio.Domain.Entities;

namespace QuinntyneBrownStudio.Application.Catalog.Studio;

public sealed record SaveStudio(DomainEntities.Studio Value, Guid? Id)
    : IRequest<DomainEntities.Studio>;
