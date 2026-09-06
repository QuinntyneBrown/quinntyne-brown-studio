using Qbs.Domain.Models;

namespace Qbs.Application.Ports;

public interface IPhotoAnalysisService
{
    Task<PhotoAnalysis> Analyze(Guid photoId, Stream preview, CancellationToken ct);
}
