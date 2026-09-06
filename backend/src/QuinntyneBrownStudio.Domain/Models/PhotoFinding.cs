using QuinntyneBrownStudio.Domain.Enums;

namespace QuinntyneBrownStudio.Domain.Models;

public sealed record PhotoFinding(string Criterion, FindingOutcome Outcome, string Explanation);
