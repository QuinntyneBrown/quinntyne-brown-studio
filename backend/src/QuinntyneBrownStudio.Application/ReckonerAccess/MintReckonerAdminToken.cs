using MediatR;

namespace QuinntyneBrownStudio.Application.ReckonerAccess;

public sealed record MintReckonerAdminToken : IRequest<ReckonerAdminSession>;
