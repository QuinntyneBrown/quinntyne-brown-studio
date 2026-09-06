using MediatR;
using Microsoft.AspNetCore.Mvc;
using QuinntyneBrownStudio.Api.Models;
using QuinntyneBrownStudio.Application.Quotations;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;

namespace QuinntyneBrownStudio.Api.Controllers;

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
