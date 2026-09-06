using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Domain.ValueObjects;

namespace QuinntyneBrownStudio.AcceptanceTests;

public sealed class QuoteRoutes : IRouteDistanceService
{
    public decimal Distance { get; set; } = 12345m;
    public ResolvedLocation[] LastRoute { get; private set; } = [];
    public Exception? Failure { get; set; }
    public ResolvedLocation[] Candidates { get; set; } = [];
    public Task<ResolvedLocation[]> Resolve(string address, CancellationToken ct) =>
        Failure is null ? Task.FromResult(Candidates) : Task.FromException<ResolvedLocation[]>(Failure);
    public Task<decimal> Metres(ResolvedLocation[] route, CancellationToken ct)
    {
        LastRoute = route;
        return Failure is null ? Task.FromResult(Distance) : Task.FromException<decimal>(Failure);
    }
}
