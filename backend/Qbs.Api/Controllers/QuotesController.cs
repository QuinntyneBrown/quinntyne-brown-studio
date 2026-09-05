using MediatR;
using Microsoft.AspNetCore.Mvc;
using Qbs.Application;
using Qbs.Domain;

namespace Qbs.Api.Controllers;

[ApiController, Route("api/public")]
public sealed class QuotesController(ISender sender, IRouteDistanceService routes) : ControllerBase
{
    [HttpPost("quotes/calculate")]
    public async Task<QuoteResult> Calculate(QuoteInput input, CancellationToken ct) =>
        await sender.Send(new CalculateQuote(input), ct);

    [HttpPost("locations/resolve")]
    public async Task<ResolvedLocation[]> Resolve(AddressInput input, CancellationToken ct)
    {
        Rules.Text(input.Address, "address");
        return await routes.Resolve(input.Address, ct);
    }
}
