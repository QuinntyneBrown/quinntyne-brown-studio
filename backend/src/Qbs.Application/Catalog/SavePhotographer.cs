using MediatR;
using Qbs.Domain;

namespace Qbs.Application;

public sealed record SavePhotographer(Photographer Value, Guid? Id) : IRequest<Photographer>;
