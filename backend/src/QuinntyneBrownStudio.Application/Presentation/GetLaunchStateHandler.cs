using MediatR;
using Microsoft.Extensions.Options;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Domain.Models;

namespace QuinntyneBrownStudio.Application.Presentation;

/// <summary>The gate applies to anonymous requesters only; a signed-in account sees the whole studio.</summary>
public sealed class GetLaunchStateHandler(IOptions<LaunchOptions> options, IAccountContext context)
    : IRequestHandler<GetLaunchState, LaunchState>
{
    public Task<LaunchState> Handle(GetLaunchState request, CancellationToken ct) =>
        Task.FromResult(new LaunchState(options.Value.ComingSoon && !context.Session().Authenticated));
}
