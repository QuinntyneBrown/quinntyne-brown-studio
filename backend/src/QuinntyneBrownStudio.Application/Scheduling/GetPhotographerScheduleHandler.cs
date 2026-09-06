using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Scheduling;

public sealed class GetPhotographerScheduleHandler(AdminCatalog catalog) : IRequestHandler<GetPhotographerSchedule, PhotographerSchedule>
{
    public async Task<PhotographerSchedule> Handle(GetPhotographerSchedule request, CancellationToken ct) =>
        await catalog.Get<PhotographerSchedule>(request.Id) ?? new PhotographerSchedule { Id = request.Id, PhotographerId = request.Id };
}
