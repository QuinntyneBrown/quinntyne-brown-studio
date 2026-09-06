using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Photos;

public sealed class GetPhotoPreviewHandler(PhotoWorkflows photos) : IRequestHandler<GetPhotoPreview, MediaFile>
{
    public Task<MediaFile> Handle(GetPhotoPreview request, CancellationToken ct) =>
        photos.Preview(request.Id, request.Client, request.Slug, ct, request.Thumbnail);
}
