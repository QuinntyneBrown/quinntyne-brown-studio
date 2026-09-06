using MediatR;
using Qbs.Application.Ports;
using Qbs.Domain.Policies;
using Qbs.Domain.ValueObjects;

namespace Qbs.Application.Quotations;

public sealed class ResolveQuoteLocationHandler(IRouteDistanceService routes)
    : IRequestHandler<ResolveQuoteLocation, ResolvedLocation[]>
{
    public Task<ResolvedLocation[]> Handle(ResolveQuoteLocation request, CancellationToken ct)
    {
        Rules.Text(request.Address, "address");
        return routes.Resolve(request.Address.Trim(), ct);
    }
}
