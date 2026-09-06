using QuinntyneBrownStudio.Domain.ValueObjects;

namespace QuinntyneBrownStudio.Application.Ports;

public interface IRouteDistanceService
{
    Task<ResolvedLocation[]> Resolve(string address, CancellationToken ct);
    Task<decimal> Metres(ResolvedLocation[] orderedRoute, CancellationToken ct);
}
