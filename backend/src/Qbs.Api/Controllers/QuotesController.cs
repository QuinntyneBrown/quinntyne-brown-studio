using MediatR;
using Microsoft.AspNetCore.Mvc;
using Qbs.Api.Models;
using Qbs.Application.Quotations;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;

namespace Qbs.Api.Controllers;

[ApiController, Route("api/public")]
public sealed class QuotesController(ISender sender) : ControllerBase
{
    [HttpPost("quotes/calculate")]
    public async Task<QuoteResult> Calculate(QuoteInput input, CancellationToken ct) =>
        await sender.Send(new CalculateQuote(input), ct);

    [HttpPost("locations/resolve")]
    public Task<ResolvedLocation[]> Resolve(AddressInput input, CancellationToken ct) =>
        sender.Send(new ResolveQuoteLocation(input.Address), ct);

    [HttpGet("studios")]
    public Task<QuoteStudioOption[]> Studios(CancellationToken ct) => sender.Send(new GetQuoteStudios(), ct);
}
