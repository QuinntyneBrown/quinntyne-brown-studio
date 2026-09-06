using MediatR;
using DomainEntities = Qbs.Domain.Entities;

namespace Qbs.Application.Catalog.Equipment;

public sealed class SaveEquipmentHandler(AdminCatalog catalog)
    : IRequestHandler<SaveEquipment, DomainEntities.Equipment>
{
    public Task<DomainEntities.Equipment> Handle(SaveEquipment request, CancellationToken ct) =>
        catalog.Save(request.Value, request.Id);
}
