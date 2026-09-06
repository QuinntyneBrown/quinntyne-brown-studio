using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Catalog.Studio;

public sealed class ListStudioHandler(AdminCatalog catalog) : IRequestHandler<ListStudio, Qbs.Domain.Entities.Studio[]>
{
    public Task<Qbs.Domain.Entities.Studio[]> Handle(ListStudio request, CancellationToken ct) =>
        catalog.List<Qbs.Domain.Entities.Studio>();
}
