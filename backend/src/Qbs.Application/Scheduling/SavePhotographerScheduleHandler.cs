using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Scheduling;

public sealed class SavePhotographerScheduleHandler(Scheduling scheduling) : IRequestHandler<SavePhotographerSchedule, PhotographerSchedule>
{
    public Task<PhotographerSchedule> Handle(SavePhotographerSchedule request, CancellationToken ct) =>
        scheduling.Save(request.Id, request.Value);
}
