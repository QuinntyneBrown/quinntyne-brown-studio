using Qbs.Domain.ValueObjects;

namespace Qbs.Application.Ports;

public interface IRouteDistanceService
{
    Task<ResolvedLocation[]> Resolve(string address, CancellationToken ct);
    Task<decimal> Metres(ResolvedLocation[] orderedRoute, CancellationToken ct);
}
