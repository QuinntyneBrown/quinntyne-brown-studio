using Qbs.Domain;

namespace Qbs.Application;

public interface IRouteDistanceService
{
    Task<ResolvedLocation[]> Resolve(string address, CancellationToken ct);
    Task<decimal> Metres(ResolvedLocation[] orderedRoute, CancellationToken ct);
}
