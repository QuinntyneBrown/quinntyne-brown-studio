namespace QuinntyneBrownStudio.Application.ReckonerAccess;

public sealed record ReckonerAdminSession(string ApiBaseUrl, string AdminToken, DateTimeOffset ExpiresAt);
