using MediatR;
namespace Qbs.Application.Diagnostics;
public sealed record GetDevelopmentMail : IRequest<object>;
