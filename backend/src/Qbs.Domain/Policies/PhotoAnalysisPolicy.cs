using Qbs.Domain.Exceptions;
using Qbs.Domain.Models;
namespace Qbs.Domain.Policies;

public static class PhotoAnalysisPolicy
{
    public static PhotoAnalysis Validate(Guid photoId, PhotoAnalysis? result)
    {
        string[] criteria = ["sharpness", "exposure", "closed-eyes"];
        if (result == null || result.PhotoId != photoId || result.Findings == null || result.Findings.Length != criteria.Length
            || string.IsNullOrWhiteSpace(result.Recommendation) || string.IsNullOrWhiteSpace(result.ModelVersion) || string.IsNullOrWhiteSpace(result.PromptVersion)
            || result.Findings.Any(finding => finding == null || !criteria.Contains(finding.Criterion) || !Enum.IsDefined(finding.Outcome) || string.IsNullOrWhiteSpace(finding.Explanation))
            || result.Findings.Select(finding => finding.Criterion).Distinct().Count() != criteria.Length)
            throw new StudioException(503, "AI findings could not be validated. Continue reviewing photographs manually.");
        return result;
    }
}
