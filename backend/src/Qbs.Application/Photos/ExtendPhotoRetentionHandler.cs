using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Photos;

public sealed class ExtendPhotoRetentionHandler(RetentionWorkflows retention) : IRequestHandler<ExtendPhotoRetention, Session>
{
    public Task<Session> Handle(ExtendPhotoRetention request, CancellationToken ct) =>
        retention.Extend(request.Id, request.Months, request.ExpiresAt, request.ExpectedVersion);
}
