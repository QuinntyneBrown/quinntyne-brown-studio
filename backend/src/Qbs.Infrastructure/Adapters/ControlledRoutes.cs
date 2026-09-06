using Qbs.Application.Ports;
using Qbs.Domain.ValueObjects;

namespace Qbs.Infrastructure.Adapters;

public sealed class ControlledRoutes : IRouteDistanceService
{
    public Task<ResolvedLocation[]> Resolve(string address, CancellationToken ct) =>
        Task.FromResult(
            new[]
            {
                new ResolvedLocation
                {
                    Label = "Controlled example: " + address,
                    Latitude = 43.65m,
                    Longitude = -79.38m,
                },
            }
        );

    public Task<decimal> Metres(ResolvedLocation[] route, CancellationToken ct) =>
        Task.FromResult(12000m);
}
