using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Catalog.Promotion;

public sealed class ListPromotionHandler(AdminCatalog catalog) : IRequestHandler<ListPromotion, QuinntyneBrownStudio.Domain.Entities.Promotion[]>
{
    public Task<QuinntyneBrownStudio.Domain.Entities.Promotion[]> Handle(ListPromotion request, CancellationToken ct) =>
        catalog.List<QuinntyneBrownStudio.Domain.Entities.Promotion>();
}
