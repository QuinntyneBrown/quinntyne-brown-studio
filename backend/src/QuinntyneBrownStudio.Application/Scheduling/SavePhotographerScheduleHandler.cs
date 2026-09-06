using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Scheduling;

public sealed class SavePhotographerScheduleHandler(Scheduling scheduling) : IRequestHandler<SavePhotographerSchedule, PhotographerSchedule>
{
    public Task<PhotographerSchedule> Handle(SavePhotographerSchedule request, CancellationToken ct) =>
        scheduling.Save(request.Id, request.Value);
}
