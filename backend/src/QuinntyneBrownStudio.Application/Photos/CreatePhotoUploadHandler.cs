using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Photos;

public sealed class CreatePhotoUploadHandler(PhotoWorkflows photos) : IRequestHandler<CreatePhotoUpload, object>
{
    public Task<object> Handle(CreatePhotoUpload request, CancellationToken ct) =>
        photos.Create(request.SessionId, request.Files, ct);
}
