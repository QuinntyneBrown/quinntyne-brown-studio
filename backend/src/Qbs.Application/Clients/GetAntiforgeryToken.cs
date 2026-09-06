using MediatR;
using Qbs.Domain.Models;
namespace Qbs.Application.Clients;
public sealed record GetAntiforgeryToken : IRequest<AntiforgeryToken>;
