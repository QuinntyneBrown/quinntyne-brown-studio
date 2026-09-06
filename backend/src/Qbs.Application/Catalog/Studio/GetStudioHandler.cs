using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Catalog.Studio;

public sealed class GetStudioHandler(AdminCatalog catalog) : IRequestHandler<GetStudio, Qbs.Domain.Entities.Studio>
{
    public async Task<Qbs.Domain.Entities.Studio> Handle(GetStudio request, CancellationToken ct) =>
        await catalog.Get<Qbs.Domain.Entities.Studio>(request.Id) ?? throw new StudioException(404, "Record not found.");
}
