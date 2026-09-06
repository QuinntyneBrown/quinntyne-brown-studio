using MediatR;
using DomainEntities = Qbs.Domain.Entities;

namespace Qbs.Application.Catalog.Equipment;

public sealed record SaveEquipment(DomainEntities.Equipment Value, Guid? Id)
    : IRequest<DomainEntities.Equipment>;
