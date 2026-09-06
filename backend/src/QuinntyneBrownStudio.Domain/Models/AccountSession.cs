namespace QuinntyneBrownStudio.Domain.Models;

public sealed record AccountSession(bool Authenticated, string? Id, string[] Roles);
