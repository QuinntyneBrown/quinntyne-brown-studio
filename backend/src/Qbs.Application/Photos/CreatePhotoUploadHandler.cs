using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Photos;

public sealed class CreatePhotoUploadHandler(PhotoWorkflows photos) : IRequestHandler<CreatePhotoUpload, object>
{
    public Task<object> Handle(CreatePhotoUpload request, CancellationToken ct) =>
        photos.Create(request.SessionId, request.Files, ct);
}
