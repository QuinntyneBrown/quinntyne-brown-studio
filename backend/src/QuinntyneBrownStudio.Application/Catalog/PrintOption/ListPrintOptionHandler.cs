using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Catalog.PrintOption;

public sealed class ListPrintOptionHandler(AdminCatalog catalog) : IRequestHandler<ListPrintOption, QuinntyneBrownStudio.Domain.Entities.PrintOption[]>
{
    public Task<QuinntyneBrownStudio.Domain.Entities.PrintOption[]> Handle(ListPrintOption request, CancellationToken ct) =>
        catalog.List<QuinntyneBrownStudio.Domain.Entities.PrintOption>();
}
