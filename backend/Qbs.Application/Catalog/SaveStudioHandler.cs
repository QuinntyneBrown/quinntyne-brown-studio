using MediatR;
using Qbs.Domain;

namespace Qbs.Application;

public sealed class SaveStudioHandler(AdminCatalog catalog) : IRequestHandler<SaveStudio, Studio>
{
    public Task<Studio> Handle(SaveStudio request, CancellationToken ct) =>
        catalog.Save(request.Value, request.Id);
}
