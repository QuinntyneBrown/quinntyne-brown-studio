using MediatR;
namespace Qbs.Application.Diagnostics;
public sealed record GetHealth : IRequest<object>;
