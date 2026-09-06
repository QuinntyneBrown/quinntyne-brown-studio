using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Scheduling;

public sealed class GetPhotographerScheduleHandler(AdminCatalog catalog) : IRequestHandler<GetPhotographerSchedule, PhotographerSchedule>
{
    public async Task<PhotographerSchedule> Handle(GetPhotographerSchedule request, CancellationToken ct) =>
        await catalog.Get<PhotographerSchedule>(request.Id) ?? new PhotographerSchedule { Id = request.Id, PhotographerId = request.Id };
}
