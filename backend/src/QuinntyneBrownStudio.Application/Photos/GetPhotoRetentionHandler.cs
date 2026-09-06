using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Photos;

public sealed class GetPhotoRetentionHandler(RetentionWorkflows retention) : IRequestHandler<GetPhotoRetention, object>
{
    public Task<object> Handle(GetPhotoRetention request, CancellationToken ct) =>
        retention.Get(request.Id);
}
