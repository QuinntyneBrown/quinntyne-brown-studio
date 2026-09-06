using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Photos;

public sealed class GetPhotoRetentionHandler(RetentionWorkflows retention) : IRequestHandler<GetPhotoRetention, object>
{
    public Task<object> Handle(GetPhotoRetention request, CancellationToken ct) =>
        retention.Get(request.Id);
}
