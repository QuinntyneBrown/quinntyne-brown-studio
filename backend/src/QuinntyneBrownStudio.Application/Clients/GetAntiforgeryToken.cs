using MediatR;
using QuinntyneBrownStudio.Domain.Models;
namespace QuinntyneBrownStudio.Application.Clients;
public sealed record GetAntiforgeryToken : IRequest<AntiforgeryToken>;
