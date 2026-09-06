using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Catalog.Equipment;

public sealed class ListEquipmentHandler(AdminCatalog catalog) : IRequestHandler<ListEquipment, Qbs.Domain.Entities.Equipment[]>
{
    public Task<Qbs.Domain.Entities.Equipment[]> Handle(ListEquipment request, CancellationToken ct) =>
        catalog.List<Qbs.Domain.Entities.Equipment>();
}
