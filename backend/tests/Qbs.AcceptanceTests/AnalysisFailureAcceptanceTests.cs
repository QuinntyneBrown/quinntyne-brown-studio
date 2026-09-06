using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Qbs.Application.Ports;
using Qbs.Domain.Entities;
using Qbs.Domain.Enums;
using Qbs.Domain.Models;
using Qbs.Infrastructure.Processing;

namespace Qbs.AcceptanceTests;

public sealed class AnalysisFailureAcceptanceTests
{
    // Given malformed, wrong-photo or unavailable advice, when processing exhausts
    // retries, then the API reports failure without storing advice or blocking manual review.
    [Theory]
    [InlineData("wrong-photo")]
    [InlineData("missing-criterion")]
    [InlineData("duplicate-criterion")]
    [InlineData("unknown-outcome")]
    [InlineData("no-recommendation")]
    [InlineData("no-provenance")]
    [InlineData("outage")]
    public async Task AC_L2_030_02_AC_L2_031_01_AC_L2_067_01_Invalid_advice_preserves_manual_review(string mode)
    {
        using var factory = new StudioFactory { PhotoAnalysis = new ControlledAnalysisResult(id => Result(id, mode)) };
        using var admin = await factory.Actor();
        await using var scope = factory.Services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IStudioStore>();
        var session = new Session { Name = "Manual review remains available" };
        var photo = new SessionPhoto { SessionId = session.Id, Name = "portrait.jpg", State = PhotoState.Ready, PreviewKey = "previews/" + Guid.NewGuid() };
        await store.Run("fixture", async tx => { await tx.Save(session, 0); await tx.Save(photo, 0); return true; });
        await scope.ServiceProvider.GetRequiredService<IPhotoStorage>().Write(photo.PreviewKey!, new MemoryStream([255, 216, 255]), CancellationToken.None);
        var started = await admin.PostAsJsonAsync($"/api/admin/sessions/{session.Id}/analysis", new { photoIds = new[] { photo.Id } });
        started.EnsureSuccessStatusCode();
        var batchId = (await started.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        var job = Assert.Single(await store.Run("fixture", tx => tx.List<BackgroundJob>()));
        await store.Run("fixture", async tx => { job.Attempt = 4; await tx.Save(job, job.Version); return true; });
        await scope.ServiceProvider.GetRequiredService<JobProcessor>().Process(job.Id, CancellationToken.None);
        var status = await admin.GetFromJsonAsync<JsonElement>($"/api/admin/analysis/{batchId}");
        var result = status.GetProperty("photos")[0];
        Assert.Equal("Failed", result.GetProperty("state").GetString());
        Assert.Equal(JsonValueKind.Null, result.GetProperty("result").ValueKind);
        Assert.NotEmpty(result.GetProperty("error").GetString()!);
        Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync($"/api/admin/photos/{photo.Id}/preview")).StatusCode);
        Assert.Empty(await store.Run("fixture", tx => tx.List<PublicGallery>()));
        var retry = await admin.PostAsJsonAsync($"/api/admin/analysis/{batchId}/retry", new { failedPhotoIds = new[] { photo.Id } });
        retry.EnsureSuccessStatusCode();
    }

    private static PhotoAnalysis Result(Guid id, string mode)
    {
        if (mode == "outage") throw new HttpRequestException("Controlled outage.");
        var findings = new[] { "sharpness", "exposure", "closed-eyes" }.Select(criterion => new PhotoFinding(criterion, FindingOutcome.Uncertain, "Review this criterion manually.")).ToArray();
        return new(mode == "wrong-photo" ? Guid.NewGuid() : id,
            mode == "missing-criterion" ? findings[..1] : mode == "duplicate-criterion" ? [findings[0], findings[0], findings[0]] : mode == "unknown-outcome" ? [findings[0] with { Outcome = (FindingOutcome)999 }, findings[1], findings[2]] : findings,
            mode == "no-recommendation" ? "" : "Review manually.", mode == "no-provenance" ? "" : "controlled-model", "rubric-v1");
    }
}
