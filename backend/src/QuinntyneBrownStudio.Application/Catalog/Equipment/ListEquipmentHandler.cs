using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Catalog.Equipment;

public sealed class ListEquipmentHandler(AdminCatalog catalog) : IRequestHandler<ListEquipment, QuinntyneBrownStudio.Domain.Entities.Equipment[]>
{
    public Task<QuinntyneBrownStudio.Domain.Entities.Equipment[]> Handle(ListEquipment request, CancellationToken ct) =>
        catalog.List<QuinntyneBrownStudio.Domain.Entities.Equipment>();
}
