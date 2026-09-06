using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Catalog.Equipment;

public sealed class GetEquipmentHandler(AdminCatalog catalog) : IRequestHandler<GetEquipment, Qbs.Domain.Entities.Equipment>
{
    public async Task<Qbs.Domain.Entities.Equipment> Handle(GetEquipment request, CancellationToken ct) =>
        await catalog.Get<Qbs.Domain.Entities.Equipment>(request.Id) ?? throw new StudioException(404, "Record not found.");
}
