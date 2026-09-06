using Qbs.Domain.Enums;

namespace Qbs.Domain.Models;

public sealed record PhotoFinding(string Criterion, FindingOutcome Outcome, string Explanation);
