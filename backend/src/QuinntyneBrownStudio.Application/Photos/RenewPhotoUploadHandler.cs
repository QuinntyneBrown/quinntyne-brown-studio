using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Photos;

public sealed class RenewPhotoUploadHandler(PhotoWorkflows photos) : IRequestHandler<RenewPhotoUpload, StorageGrant>
{
    public Task<StorageGrant> Handle(RenewPhotoUpload request, CancellationToken ct) =>
        photos.Renew(request.BatchId, request.PhotoId, ct);
}
