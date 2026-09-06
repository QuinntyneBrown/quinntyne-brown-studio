using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Photos;

public sealed class GetPhotoUploadHandler(PhotoWorkflows photos) : IRequestHandler<GetPhotoUpload, object>
{
    public Task<object> Handle(GetPhotoUpload request, CancellationToken ct) =>
        photos.Status(request.Id);
}
