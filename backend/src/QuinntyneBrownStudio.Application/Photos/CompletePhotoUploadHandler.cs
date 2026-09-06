using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Photos;

public sealed class CompletePhotoUploadHandler(PhotoWorkflows photos) : IRequestHandler<CompletePhotoUpload, object>
{
    public Task<object> Handle(CompletePhotoUpload request, CancellationToken ct) =>
        photos.Finalize(request.BatchId, request.PhotoId, ct);
}
