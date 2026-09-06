using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Catalog.Equipment;

public sealed class GetEquipmentHandler(AdminCatalog catalog) : IRequestHandler<GetEquipment, QuinntyneBrownStudio.Domain.Entities.Equipment>
{
    public async Task<QuinntyneBrownStudio.Domain.Entities.Equipment> Handle(GetEquipment request, CancellationToken ct) =>
        await catalog.Get<QuinntyneBrownStudio.Domain.Entities.Equipment>(request.Id) ?? throw new StudioException(404, "Record not found.");
}
