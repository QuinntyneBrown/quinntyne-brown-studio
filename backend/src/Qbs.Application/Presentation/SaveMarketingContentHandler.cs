using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Presentation;

public sealed class SaveMarketingContentHandler(Presentation presentation) : IRequestHandler<SaveMarketingContent, MarketingContent>
{
    public Task<MarketingContent> Handle(SaveMarketingContent request, CancellationToken ct) =>
        presentation.Save(request.Key, request.Value);
}
