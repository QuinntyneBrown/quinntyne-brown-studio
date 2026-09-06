using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Domain.Policies;
using QuinntyneBrownStudio.Domain.ValueObjects;

namespace QuinntyneBrownStudio.Application.Quotations;

public sealed class ResolveQuoteLocationHandler(IRouteDistanceService routes)
    : IRequestHandler<ResolveQuoteLocation, ResolvedLocation[]>
{
    public Task<ResolvedLocation[]> Handle(ResolveQuoteLocation request, CancellationToken ct)
    {
        Rules.Text(request.Address, "address");
        return routes.Resolve(request.Address.Trim(), ct);
    }
}
