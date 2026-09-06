using System.Text.Json;
namespace Qbs.Qualification;

public interface IQualificationCheck
{
    Task<QualificationReport> Run(JsonElement manifest, string directory, string reportPath, CancellationToken ct);
}
