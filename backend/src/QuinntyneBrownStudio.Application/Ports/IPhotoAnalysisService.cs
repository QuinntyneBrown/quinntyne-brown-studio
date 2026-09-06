using QuinntyneBrownStudio.Domain.Models;

namespace QuinntyneBrownStudio.Application.Ports;

public interface IPhotoAnalysisService
{
    Task<PhotoAnalysis> Analyze(Guid photoId, Stream preview, CancellationToken ct);
}
