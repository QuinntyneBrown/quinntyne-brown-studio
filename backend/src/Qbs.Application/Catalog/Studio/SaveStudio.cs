using MediatR;
using DomainEntities = Qbs.Domain.Entities;

namespace Qbs.Application.Catalog.Studio;

public sealed record SaveStudio(DomainEntities.Studio Value, Guid? Id)
    : IRequest<DomainEntities.Studio>;
