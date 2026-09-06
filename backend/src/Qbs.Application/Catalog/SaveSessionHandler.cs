using MediatR;
using Qbs.Domain;

namespace Qbs.Application;

public sealed class SaveSessionHandler(AdminCatalog catalog) : IRequestHandler<SaveSession, Session>
{
    public Task<Session> Handle(SaveSession request, CancellationToken ct) =>
        catalog.Save(request.Value, request.Id);
}
