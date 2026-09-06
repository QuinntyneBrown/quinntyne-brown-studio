using MediatR;
using DomainEntities = QuinntyneBrownStudio.Domain.Entities;

namespace QuinntyneBrownStudio.Application.Catalog.RateConfiguration;

public sealed record SaveRateConfiguration(DomainEntities.RateConfiguration Value, Guid? Id)
    : IRequest<DomainEntities.RateConfiguration>;
