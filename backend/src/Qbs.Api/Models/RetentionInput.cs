using Qbs.Domain;

namespace Qbs.Api.Controllers;

public sealed record RetentionInput(int Months, DateTimeOffset? ExpiresAt, long ExpectedVersion);
