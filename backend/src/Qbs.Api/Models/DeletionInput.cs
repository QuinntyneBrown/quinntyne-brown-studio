using Qbs.Domain;

namespace Qbs.Api.Controllers;

public sealed record DeletionInput(string ImpactRevision, bool Confirm);
