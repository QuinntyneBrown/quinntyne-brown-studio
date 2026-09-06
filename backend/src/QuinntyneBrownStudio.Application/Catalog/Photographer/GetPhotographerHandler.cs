using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Catalog.Photographer;

public sealed class GetPhotographerHandler(AdminCatalog catalog) : IRequestHandler<GetPhotographer, QuinntyneBrownStudio.Domain.Entities.Photographer>
{
    public async Task<QuinntyneBrownStudio.Domain.Entities.Photographer> Handle(GetPhotographer request, CancellationToken ct) =>
        await catalog.Get<QuinntyneBrownStudio.Domain.Entities.Photographer>(request.Id) ?? throw new StudioException(404, "Record not found.");
}
