using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Catalog.Studio;

public sealed class GetStudioHandler(AdminCatalog catalog) : IRequestHandler<GetStudio, QuinntyneBrownStudio.Domain.Entities.Studio>
{
    public async Task<QuinntyneBrownStudio.Domain.Entities.Studio> Handle(GetStudio request, CancellationToken ct) =>
        await catalog.Get<QuinntyneBrownStudio.Domain.Entities.Studio>(request.Id) ?? throw new StudioException(404, "Record not found.");
}
