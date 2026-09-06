using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Clients;

public sealed class SignInAccountHandler(IIdentityAccounts accounts) : IRequestHandler<SignInAccount, object>
{
    public Task<object> Handle(SignInAccount request, CancellationToken ct) =>
        accounts.Login(request.Email, request.Password);
}
