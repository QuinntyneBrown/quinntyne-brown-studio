using Microsoft.Extensions.DependencyInjection;
using Qbs.Application.Ports;
using Qbs.Domain.Entities;
using Qbs.Domain.Enums;
namespace Qbs.AcceptanceTests;

public static class PrivatePhotoFixture
{
    public static async Task<SessionPhoto> Seed(StudioFactory factory, Guid client)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var session = new Session { Name = "Private session", ClientIds = [client], ExpiresAt = DateTimeOffset.UtcNow.AddYears(1) };
        var photo = new SessionPhoto { SessionId = session.Id, Name = "Private portrait", State = PhotoState.Ready, PreviewKey = "preview/" + Guid.NewGuid() };
        await scope.ServiceProvider.GetRequiredService<IStudioStore>().Run("fixture", async tx => { await tx.Save(session, 0); await tx.Save(photo, 0); return true; });
        await scope.ServiceProvider.GetRequiredService<IPhotoStorage>().Write(photo.PreviewKey!, new MemoryStream([255, 216, 255]), CancellationToken.None);
        return photo;
    }
}
