using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Scheduling;

public sealed class CheckSessionAvailabilityHandler(Scheduling scheduling) : IRequestHandler<CheckSessionAvailability, AvailabilityResult>
{
    public Task<AvailabilityResult> Handle(CheckSessionAvailability request, CancellationToken ct) =>
        scheduling.Check(request.Value.StartsAt, request.Value.EndsAt, request.Value.PhotographerId);
}
