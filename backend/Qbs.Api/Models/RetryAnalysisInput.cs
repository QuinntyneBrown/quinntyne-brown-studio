using Qbs.Domain;

namespace Qbs.Api.Controllers;

public sealed record RetryAnalysisInput(Guid[] FailedPhotoIds);
