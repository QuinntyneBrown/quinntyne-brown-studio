using MediatR;
using QuinntyneBrownStudio.Domain.Models;
namespace QuinntyneBrownStudio.Application.Clients;
public sealed record GetAccountSession : IRequest<AccountSession>;
