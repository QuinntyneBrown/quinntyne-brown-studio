using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Scheduling;

public sealed class CheckSessionAvailabilityHandler(Scheduling scheduling) : IRequestHandler<CheckSessionAvailability, AvailabilityResult>
{
    public Task<AvailabilityResult> Handle(CheckSessionAvailability request, CancellationToken ct) =>
        scheduling.Check(request.Value.StartsAt, request.Value.EndsAt, request.Value.PhotographerId);
}
