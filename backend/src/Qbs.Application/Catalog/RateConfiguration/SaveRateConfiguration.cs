using MediatR;
using DomainEntities = Qbs.Domain.Entities;

namespace Qbs.Application.Catalog.RateConfiguration;

public sealed record SaveRateConfiguration(DomainEntities.RateConfiguration Value, Guid? Id)
    : IRequest<DomainEntities.RateConfiguration>;
