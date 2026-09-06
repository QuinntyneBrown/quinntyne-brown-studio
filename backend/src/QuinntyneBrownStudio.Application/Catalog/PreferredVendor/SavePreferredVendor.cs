using MediatR;
using DomainEntities = QuinntyneBrownStudio.Domain.Entities;

namespace QuinntyneBrownStudio.Application.Catalog.PreferredVendor;

public sealed record SavePreferredVendor(DomainEntities.PreferredVendor Value, Guid? Id)
    : IRequest<DomainEntities.PreferredVendor>;
