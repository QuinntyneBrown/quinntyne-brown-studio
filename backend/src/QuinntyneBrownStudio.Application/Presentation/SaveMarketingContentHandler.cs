using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Presentation;

public sealed class SaveMarketingContentHandler(Presentation presentation) : IRequestHandler<SaveMarketingContent, MarketingContent>
{
    public Task<MarketingContent> Handle(SaveMarketingContent request, CancellationToken ct) =>
        presentation.Save(request.Key, request.Value);
}
