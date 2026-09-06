using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Catalog.PreferredVendor;

public sealed class GetPreferredVendorHandler(AdminCatalog catalog) : IRequestHandler<GetPreferredVendor, QuinntyneBrownStudio.Domain.Entities.PreferredVendor>
{
    public async Task<QuinntyneBrownStudio.Domain.Entities.PreferredVendor> Handle(GetPreferredVendor request, CancellationToken ct) =>
        await catalog.Get<QuinntyneBrownStudio.Domain.Entities.PreferredVendor>(request.Id) ?? throw new StudioException(404, "Record not found.");
}
