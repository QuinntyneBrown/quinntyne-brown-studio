using Qbs.Domain.ValueObjects;

namespace Qbs.Domain.Entities;

public sealed class PhotographerSchedule : Entity
{
    public Guid PhotographerId { get; set; }
    public TimeWindow[] WorkingWindows { get; set; } = [];
    public TimeWindow[] UnavailableWindows { get; set; } = [];
    public TravelBuffers Buffers { get; set; } = new();
}
