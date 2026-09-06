using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;

namespace QuinntyneBrownStudio.Application.Quotations;

public sealed class GetQuoteStudiosHandler(IStudioStore store) : IRequestHandler<GetQuoteStudios, QuoteStudioOption[]>
{
    public Task<QuoteStudioOption[]> Handle(GetQuoteStudios request, CancellationToken ct) =>
        store.Run("pricing", async tx => (await tx.List<Studio>()).Where(x => x.Enabled)
            .Select(x => new QuoteStudioOption(x.Id, x.Name, x.ResolvedAddress, x.HourlyFee)).ToArray(), ct);
}
