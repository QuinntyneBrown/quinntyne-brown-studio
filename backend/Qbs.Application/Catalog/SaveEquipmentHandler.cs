using MediatR;
using Qbs.Domain;

namespace Qbs.Application;

public sealed class SaveEquipmentHandler(AdminCatalog catalog)
    : IRequestHandler<SaveEquipment, Equipment>
{
    public Task<Equipment> Handle(SaveEquipment request, CancellationToken ct) =>
        catalog.Save(request.Value, request.Id);
}
