using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Photos;

public sealed class DeleteSessionPhotosHandler(RetentionWorkflows retention) : IRequestHandler<DeleteSessionPhotos, object>
{
    public Task<object> Handle(DeleteSessionPhotos request, CancellationToken ct) =>
        retention.Delete(request.Id, request.ImpactRevision, request.Confirm);
}
