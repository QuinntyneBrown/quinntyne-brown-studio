using MediatR;
using DomainEntities = Qbs.Domain.Entities;

namespace Qbs.Application.Catalog.PrintOption;

public sealed class SavePrintOptionHandler(AdminCatalog catalog)
    : IRequestHandler<SavePrintOption, DomainEntities.PrintOption>
{
    public Task<DomainEntities.PrintOption> Handle(SavePrintOption request, CancellationToken ct) =>
        catalog.Save(request.Value, request.Id);
}
