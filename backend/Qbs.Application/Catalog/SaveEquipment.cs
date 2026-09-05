using MediatR;
using Qbs.Domain;

namespace Qbs.Application;

public sealed record SaveEquipment(Equipment Value, Guid? Id) : IRequest<Equipment>;
