using MediatR;
using Qbs.Domain;

namespace Qbs.Application;

public sealed record SaveSession(Session Value, Guid? Id) : IRequest<Session>;
