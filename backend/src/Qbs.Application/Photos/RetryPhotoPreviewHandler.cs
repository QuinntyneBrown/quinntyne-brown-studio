using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Photos;

public sealed class RetryPhotoPreviewHandler(PhotoWorkflows photos) : IRequestHandler<RetryPhotoPreview, object>
{
    public Task<object> Handle(RetryPhotoPreview request, CancellationToken ct) =>
        photos.Retry(request.Id);
}
