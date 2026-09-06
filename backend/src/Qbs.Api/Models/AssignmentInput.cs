namespace Qbs.Api.Controllers;

public sealed record AssignmentInput(Guid[] ClientIds, long ExpectedVersion);
