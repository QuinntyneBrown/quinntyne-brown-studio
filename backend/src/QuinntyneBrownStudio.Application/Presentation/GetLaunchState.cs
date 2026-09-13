using MediatR;
using QuinntyneBrownStudio.Domain.Models;

namespace QuinntyneBrownStudio.Application.Presentation;

public sealed record GetLaunchState : IRequest<LaunchState>;
