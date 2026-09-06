using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Photos;

public sealed class CompletePhotoUploadHandler(PhotoWorkflows photos) : IRequestHandler<CompletePhotoUpload, object>
{
    public Task<object> Handle(CompletePhotoUpload request, CancellationToken ct) =>
        photos.Finalize(request.BatchId, request.PhotoId, ct);
}
