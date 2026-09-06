using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Catalog.PrintOption;

public sealed class GetPrintOptionHandler(AdminCatalog catalog) : IRequestHandler<GetPrintOption, QuinntyneBrownStudio.Domain.Entities.PrintOption>
{
    public async Task<QuinntyneBrownStudio.Domain.Entities.PrintOption> Handle(GetPrintOption request, CancellationToken ct) =>
        await catalog.Get<QuinntyneBrownStudio.Domain.Entities.PrintOption>(request.Id) ?? throw new StudioException(404, "Record not found.");
}
