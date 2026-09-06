using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Catalog.Session;

public sealed class GetSessionHandler(AdminCatalog catalog) : IRequestHandler<GetSession, QuinntyneBrownStudio.Domain.Entities.Session>
{
    public async Task<QuinntyneBrownStudio.Domain.Entities.Session> Handle(GetSession request, CancellationToken ct) =>
        await catalog.Get<QuinntyneBrownStudio.Domain.Entities.Session>(request.Id) ?? throw new StudioException(404, "Record not found.");
}
