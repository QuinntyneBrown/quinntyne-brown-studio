using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Catalog.PublicGallery;

public sealed class ListPublicGalleryHandler(AdminCatalog catalog) : IRequestHandler<ListPublicGallery, Qbs.Domain.Entities.PublicGallery[]>
{
    public Task<Qbs.Domain.Entities.PublicGallery[]> Handle(ListPublicGallery request, CancellationToken ct) =>
        catalog.List<Qbs.Domain.Entities.PublicGallery>();
}
