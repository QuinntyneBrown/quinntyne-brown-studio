namespace QuinntyneBrownStudio.Application.ReckonerAccess;

public interface IReckonerAccess
{
    Task<ReckonerAdminSession> Mint(CancellationToken cancellationToken);
}
