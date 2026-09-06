using MediatR;
using DomainEntities = QuinntyneBrownStudio.Domain.Entities;

namespace QuinntyneBrownStudio.Application.Catalog.Equipment;

public sealed record SaveEquipment(DomainEntities.Equipment Value, Guid? Id)
    : IRequest<DomainEntities.Equipment>;
