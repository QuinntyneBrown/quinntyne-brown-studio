namespace Qbs.Domain.Models;

public sealed record AccountSession(bool Authenticated, string? Id, string[] Roles);
