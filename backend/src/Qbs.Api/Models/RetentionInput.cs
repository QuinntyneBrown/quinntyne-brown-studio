namespace Qbs.Api.Models;

public sealed record RetentionInput(int Months, DateTimeOffset? ExpiresAt, long ExpectedVersion);
