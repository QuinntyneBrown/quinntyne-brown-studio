using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Photos;

public sealed class ListSessionPhotosHandler(PhotoWorkflows photos) : IRequestHandler<ListSessionPhotos, object>
{
    public Task<object> Handle(ListSessionPhotos request, CancellationToken ct) =>
        photos.Photos(request.Id, request.Cursor);
}
