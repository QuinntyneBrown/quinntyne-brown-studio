using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Catalog.PublicGallery;

public sealed class GetPublicGalleryHandler(AdminCatalog catalog) : IRequestHandler<GetPublicGallery, QuinntyneBrownStudio.Domain.Entities.PublicGallery>
{
    public async Task<QuinntyneBrownStudio.Domain.Entities.PublicGallery> Handle(GetPublicGallery request, CancellationToken ct) =>
        await catalog.Get<QuinntyneBrownStudio.Domain.Entities.PublicGallery>(request.Id) ?? throw new StudioException(404, "Record not found.");
}
