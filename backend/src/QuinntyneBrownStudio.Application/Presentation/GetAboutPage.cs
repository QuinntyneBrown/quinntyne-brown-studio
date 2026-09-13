using MediatR;

namespace QuinntyneBrownStudio.Application.Presentation;

public sealed record GetAboutPage : IRequest<AboutPageView>;
