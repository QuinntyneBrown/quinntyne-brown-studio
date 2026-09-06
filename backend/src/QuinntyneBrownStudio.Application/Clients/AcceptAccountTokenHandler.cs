using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Clients;

public sealed class AcceptAccountTokenHandler(IIdentityAccounts accounts) : IRequestHandler<AcceptAccountToken, object>
{
    public Task<object> Handle(AcceptAccountToken request, CancellationToken ct) =>
        accounts.Accept(request.Token, request.Password, request.Purpose);
}
