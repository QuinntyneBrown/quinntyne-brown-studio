using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Photos;

public sealed class GetPhotoPreviewHandler(PhotoWorkflows photos) : IRequestHandler<GetPhotoPreview, MediaFile>
{
    public Task<MediaFile> Handle(GetPhotoPreview request, CancellationToken ct) =>
        photos.Preview(request.Id, request.Client, request.Slug, ct, request.Thumbnail);
}
