using Qbs.Application.Ports;
using Qbs.Domain.Models;
namespace Qbs.AcceptanceTests;

public sealed class ControlledAnalysisResult(Func<Guid, PhotoAnalysis> result) : IPhotoAnalysisService
{
    public Task<PhotoAnalysis> Analyze(Guid id, Stream preview, CancellationToken ct) => Task.FromResult(result(id));
}
