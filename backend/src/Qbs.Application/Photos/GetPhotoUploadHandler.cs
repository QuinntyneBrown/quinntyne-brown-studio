using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Photos;

public sealed class GetPhotoUploadHandler(PhotoWorkflows photos) : IRequestHandler<GetPhotoUpload, object>
{
    public Task<object> Handle(GetPhotoUpload request, CancellationToken ct) =>
        photos.Status(request.Id);
}
