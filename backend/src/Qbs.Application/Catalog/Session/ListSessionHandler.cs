using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Catalog.Session;

public sealed class ListSessionHandler(AdminCatalog catalog) : IRequestHandler<ListSession, Qbs.Domain.Entities.Session[]>
{
    public Task<Qbs.Domain.Entities.Session[]> Handle(ListSession request, CancellationToken ct) =>
        catalog.List<Qbs.Domain.Entities.Session>();
}
