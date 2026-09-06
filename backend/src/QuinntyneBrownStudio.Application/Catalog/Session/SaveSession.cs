using MediatR;
using DomainEntities = QuinntyneBrownStudio.Domain.Entities;

namespace QuinntyneBrownStudio.Application.Catalog.Session;

public sealed record SaveSession(DomainEntities.Session Value, Guid? Id)
    : IRequest<DomainEntities.Session>;
