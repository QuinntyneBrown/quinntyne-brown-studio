using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Catalog.Session;

public sealed class GetSessionHandler(AdminCatalog catalog) : IRequestHandler<GetSession, Qbs.Domain.Entities.Session>
{
    public async Task<Qbs.Domain.Entities.Session> Handle(GetSession request, CancellationToken ct) =>
        await catalog.Get<Qbs.Domain.Entities.Session>(request.Id) ?? throw new StudioException(404, "Record not found.");
}
