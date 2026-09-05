using MediatR;
using Qbs.Domain;

namespace Qbs.Application;

public sealed record SaveDiscountConfiguration(DiscountConfiguration Value, Guid? Id)
    : IRequest<DiscountConfiguration>;
