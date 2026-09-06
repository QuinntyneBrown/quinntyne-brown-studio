using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Photos;

public sealed class DeleteSessionPhotosHandler(RetentionWorkflows retention) : IRequestHandler<DeleteSessionPhotos, object>
{
    public Task<object> Handle(DeleteSessionPhotos request, CancellationToken ct) =>
        retention.Delete(request.Id, request.ImpactRevision, request.Confirm);
}
