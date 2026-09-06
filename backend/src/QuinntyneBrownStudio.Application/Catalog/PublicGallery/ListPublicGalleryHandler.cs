using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Catalog.PublicGallery;

public sealed class ListPublicGalleryHandler(AdminCatalog catalog) : IRequestHandler<ListPublicGallery, QuinntyneBrownStudio.Domain.Entities.PublicGallery[]>
{
    public Task<QuinntyneBrownStudio.Domain.Entities.PublicGallery[]> Handle(ListPublicGallery request, CancellationToken ct) =>
        catalog.List<QuinntyneBrownStudio.Domain.Entities.PublicGallery>();
}
