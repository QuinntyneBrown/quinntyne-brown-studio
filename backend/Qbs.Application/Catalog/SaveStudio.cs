using MediatR;
using Qbs.Domain;

namespace Qbs.Application;

public sealed record SaveStudio(Studio Value, Guid? Id) : IRequest<Studio>;
