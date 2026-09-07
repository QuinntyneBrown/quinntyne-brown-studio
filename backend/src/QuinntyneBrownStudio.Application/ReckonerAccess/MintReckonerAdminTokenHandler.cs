using MediatR;

namespace QuinntyneBrownStudio.Application.ReckonerAccess;

public sealed class MintReckonerAdminTokenHandler(IReckonerAccess access) : IRequestHandler<MintReckonerAdminToken, ReckonerAdminSession>
{
    public Task<ReckonerAdminSession> Handle(MintReckonerAdminToken request, CancellationToken cancellationToken) => access.Mint(cancellationToken);
}
