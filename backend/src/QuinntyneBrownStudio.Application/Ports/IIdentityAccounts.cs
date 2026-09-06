namespace QuinntyneBrownStudio.Application.Ports;

public interface IIdentityAccounts
{
    Task<object> Login(string email, string password);
    Task Logout();
    Task<object> Clients();
    Task RequireClients(Guid[] ids);
    Task<Guid> Invite(string email);
    Task Recover(string email);
    Task<object> Accept(string token, string password, string purpose);
}
