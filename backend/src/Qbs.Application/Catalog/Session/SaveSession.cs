using MediatR;
using DomainEntities = Qbs.Domain.Entities;

namespace Qbs.Application.Catalog.Session;

public sealed record SaveSession(DomainEntities.Session Value, Guid? Id)
    : IRequest<DomainEntities.Session>;
