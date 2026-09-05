using MediatR;
using Qbs.Domain;

namespace Qbs.Application;

public sealed record SavePromotion(Promotion Value, Guid? Id) : IRequest<Promotion>;
