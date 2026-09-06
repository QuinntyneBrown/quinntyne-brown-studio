using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Catalog.Photographer;

public sealed class ListPhotographerHandler(AdminCatalog catalog) : IRequestHandler<ListPhotographer, Qbs.Domain.Entities.Photographer[]>
{
    public Task<Qbs.Domain.Entities.Photographer[]> Handle(ListPhotographer request, CancellationToken ct) =>
        catalog.List<Qbs.Domain.Entities.Photographer>();
}
