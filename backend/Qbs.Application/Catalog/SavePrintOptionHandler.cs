using MediatR;
using Qbs.Domain;

namespace Qbs.Application;

public sealed class SavePrintOptionHandler(AdminCatalog catalog)
    : IRequestHandler<SavePrintOption, PrintOption>
{
    public Task<PrintOption> Handle(SavePrintOption request, CancellationToken ct) =>
        catalog.Save(request.Value, request.Id);
}
