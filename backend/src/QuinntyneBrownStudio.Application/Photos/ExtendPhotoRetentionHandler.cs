using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Photos;

public sealed class ExtendPhotoRetentionHandler(RetentionWorkflows retention) : IRequestHandler<ExtendPhotoRetention, Session>
{
    public Task<Session> Handle(ExtendPhotoRetention request, CancellationToken ct) =>
        retention.Extend(request.Id, request.Months, request.ExpiresAt, request.ExpectedVersion);
}
