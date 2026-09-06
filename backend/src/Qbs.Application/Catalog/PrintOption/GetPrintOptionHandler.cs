using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Catalog.PrintOption;

public sealed class GetPrintOptionHandler(AdminCatalog catalog) : IRequestHandler<GetPrintOption, Qbs.Domain.Entities.PrintOption>
{
    public async Task<Qbs.Domain.Entities.PrintOption> Handle(GetPrintOption request, CancellationToken ct) =>
        await catalog.Get<Qbs.Domain.Entities.PrintOption>(request.Id) ?? throw new StudioException(404, "Record not found.");
}
