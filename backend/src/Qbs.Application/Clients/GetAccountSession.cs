using MediatR;
using Qbs.Domain.Models;
namespace Qbs.Application.Clients;
public sealed record GetAccountSession : IRequest<AccountSession>;
