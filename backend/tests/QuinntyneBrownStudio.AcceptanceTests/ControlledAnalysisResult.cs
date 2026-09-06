using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Domain.Models;
namespace QuinntyneBrownStudio.AcceptanceTests;

public sealed class ControlledAnalysisResult(Func<Guid, PhotoAnalysis> result) : IPhotoAnalysisService
{
    public Task<PhotoAnalysis> Analyze(Guid id, Stream preview, CancellationToken ct) => Task.FromResult(result(id));
}
