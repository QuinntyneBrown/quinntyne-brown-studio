using Qbs.Domain;

namespace Qbs.Application;

public interface IPhotoAnalysisService
{
    Task<PhotoAnalysis> Analyze(Guid photoId, Stream preview, CancellationToken ct);
}
