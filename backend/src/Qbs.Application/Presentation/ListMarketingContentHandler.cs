using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Presentation;

public sealed class ListMarketingContentHandler(AdminCatalog catalog) : IRequestHandler<ListMarketingContent, MarketingContent[]>
{
    public Task<MarketingContent[]> Handle(ListMarketingContent request, CancellationToken ct) =>
        catalog.List<MarketingContent>();
}
