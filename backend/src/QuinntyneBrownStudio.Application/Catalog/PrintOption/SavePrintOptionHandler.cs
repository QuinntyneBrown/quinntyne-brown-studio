using MediatR;
using DomainEntities = QuinntyneBrownStudio.Domain.Entities;

namespace QuinntyneBrownStudio.Application.Catalog.PrintOption;

public sealed class SavePrintOptionHandler(AdminCatalog catalog)
    : IRequestHandler<SavePrintOption, DomainEntities.PrintOption>
{
    public Task<DomainEntities.PrintOption> Handle(SavePrintOption request, CancellationToken ct) =>
        catalog.Save(request.Value, request.Id);
}
