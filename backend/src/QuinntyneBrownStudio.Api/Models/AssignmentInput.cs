namespace QuinntyneBrownStudio.Api.Models;

public sealed record AssignmentInput(Guid[] ClientIds, long ExpectedVersion);
