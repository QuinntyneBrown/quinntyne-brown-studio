namespace Qbs.Api.Models;

public sealed record AssignmentInput(Guid[] ClientIds, long ExpectedVersion);
