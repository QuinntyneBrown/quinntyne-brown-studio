using MediatR;
using DomainEntities = QuinntyneBrownStudio.Domain.Entities;

namespace QuinntyneBrownStudio.Application.Catalog.Equipment;

public sealed class SaveEquipmentHandler(AdminCatalog catalog)
    : IRequestHandler<SaveEquipment, DomainEntities.Equipment>
{
    public Task<DomainEntities.Equipment> Handle(SaveEquipment request, CancellationToken ct) =>
        catalog.Save(request.Value, request.Id);
}
