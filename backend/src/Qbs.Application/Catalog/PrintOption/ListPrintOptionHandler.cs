using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Catalog.PrintOption;

public sealed class ListPrintOptionHandler(AdminCatalog catalog) : IRequestHandler<ListPrintOption, Qbs.Domain.Entities.PrintOption[]>
{
    public Task<Qbs.Domain.Entities.PrintOption[]> Handle(ListPrintOption request, CancellationToken ct) =>
        catalog.List<Qbs.Domain.Entities.PrintOption>();
}
