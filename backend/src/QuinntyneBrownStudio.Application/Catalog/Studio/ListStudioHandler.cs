using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Catalog.Studio;

public sealed class ListStudioHandler(AdminCatalog catalog) : IRequestHandler<ListStudio, QuinntyneBrownStudio.Domain.Entities.Studio[]>
{
    public Task<QuinntyneBrownStudio.Domain.Entities.Studio[]> Handle(ListStudio request, CancellationToken ct) =>
        catalog.List<QuinntyneBrownStudio.Domain.Entities.Studio>();
}
