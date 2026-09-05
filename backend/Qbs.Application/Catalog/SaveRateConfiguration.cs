using MediatR;
using Qbs.Domain;

namespace Qbs.Application;

public sealed record SaveRateConfiguration(RateConfiguration Value, Guid? Id)
    : IRequest<RateConfiguration>;
