using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Catalog.Promotion;

public sealed class GetPromotionHandler(AdminCatalog catalog) : IRequestHandler<GetPromotion, QuinntyneBrownStudio.Domain.Entities.Promotion>
{
    public async Task<QuinntyneBrownStudio.Domain.Entities.Promotion> Handle(GetPromotion request, CancellationToken ct) =>
        await catalog.Get<QuinntyneBrownStudio.Domain.Entities.Promotion>(request.Id) ?? throw new StudioException(404, "Record not found.");
}
