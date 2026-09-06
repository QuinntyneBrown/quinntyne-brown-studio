using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Photos;

public sealed class ListSessionPhotosHandler(PhotoWorkflows photos) : IRequestHandler<ListSessionPhotos, object>
{
    public Task<object> Handle(ListSessionPhotos request, CancellationToken ct) =>
        photos.Photos(request.Id, request.Cursor);
}
