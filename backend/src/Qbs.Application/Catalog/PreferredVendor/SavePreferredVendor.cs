using MediatR;
using DomainEntities = Qbs.Domain.Entities;

namespace Qbs.Application.Catalog.PreferredVendor;

public sealed record SavePreferredVendor(DomainEntities.PreferredVendor Value, Guid? Id)
    : IRequest<DomainEntities.PreferredVendor>;
