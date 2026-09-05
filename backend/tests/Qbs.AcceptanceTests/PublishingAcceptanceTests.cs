using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Qbs.Application;
using Qbs.Domain;

namespace Qbs.AcceptanceTests;

public sealed class PublishingAcceptanceTests
{
    [Fact]
    public async Task AC_L2_004_01_AC_L2_005_01_Drafts_and_unpublished_galleries_are_not_public()
    {
        using var factory = new StudioFactory();
        using var admin = await factory.Actor();
        using var visitor = await factory.Actor(null);
        var draft = await admin.PutAsJsonAsync(
            "/api/admin/content/home",
            new
            {
                heading = "Draft title",
                body = "Draft copy",
                publish = false,
                expectedVersion = 0,
            }
        );
        draft.EnsureSuccessStatusCode();
        var version = (await draft.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("version")
            .GetInt64();
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await visitor.GetAsync("/api/public/content/home")).StatusCode
        );
        var published = await admin.PutAsJsonAsync(
            "/api/admin/content/home",
            new
            {
                heading = "Public title",
                body = "Public copy",
                publish = true,
                expectedVersion = version,
            }
        );
        published.EnsureSuccessStatusCode();
        version = (await published.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("version")
            .GetInt64();
        (
            await admin.PutAsJsonAsync(
                "/api/admin/content/home",
                new
                {
                    heading = "Next draft",
                    body = "Private changes",
                    publish = false,
                    expectedVersion = version,
                }
            )
        ).EnsureSuccessStatusCode();
        Assert.Equal(
            "Public title",
            (await visitor.GetFromJsonAsync<JsonElement>("/api/public/content/home"))
                .GetProperty("heading")
                .GetString()
        );
        var gallery = await admin.PostAsJsonAsync(
            "/api/admin/public-galleries",
            new
            {
                title = "Draft gallery",
                slug = "draft-gallery",
                published = false,
                photoIds = Array.Empty<Guid>(),
            }
        );
        gallery.EnsureSuccessStatusCode();
        Assert.Empty(
            (await visitor.GetFromJsonAsync<JsonElement>("/api/public/galleries")).EnumerateArray()
        );
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await visitor.GetAsync("/api/public/galleries/draft-gallery")).StatusCode
        );
    }

    [Fact]
    public async Task AC_L2_018_01_AC_L2_018_02_AC_L2_019_01_Stale_or_invalid_configuration_cannot_replace_saved_rates()
    {
        using var factory = new StudioFactory();
        using var admin = await factory.Actor();
        var initial = new
        {
            serviceRates = new { Wedding = "100" },
            costRates = new { travel = "1.25" },
            expectedVersion = 0,
        };
        (await admin.PutAsJsonAsync("/api/admin/rates", initial)).EnsureSuccessStatusCode();
        Assert.Equal(
            HttpStatusCode.Conflict,
            (await admin.PutAsJsonAsync("/api/admin/rates", initial)).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.BadRequest,
            (
                await admin.PutAsJsonAsync(
                    "/api/admin/rates",
                    new { serviceRates = new { Wedding = "-1" }, expectedVersion = 1 }
                )
            ).StatusCode
        );
        Assert.Equal(
            "100.00",
            (await admin.GetFromJsonAsync<JsonElement>("/api/admin/rates"))
                .GetProperty("serviceRates")
                .GetProperty("Wedding")
                .GetString()
        );
        Assert.Equal(
            HttpStatusCode.BadRequest,
            (
                await admin.PostAsJsonAsync(
                    "/api/admin/studios",
                    new
                    {
                        name = "Unresolved",
                        hourlyFee = "10",
                        resolvedAddress = new
                        {
                            label = "",
                            latitude = 0,
                            longitude = 0,
                        },
                    }
                )
            ).StatusCode
        );
    }

    [Fact]
    public async Task AC_L2_035_01_AC_L2_036_02_Private_or_repriced_prints_create_no_request()
    {
        using var factory = new StudioFactory();
        var clientId = Guid.NewGuid();
        using var client = await factory.Actor("Client", clientId.ToString());
        await using var scope = factory.Services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IStudioStore>();
        var option = new PrintOption
        {
            Name = "Print",
            UnitPrice = 15,
            Enabled = true,
        };
        var session = new Session
        {
            ClientIds = [clientId],
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(100),
        };
        var photo = new SessionPhoto { SessionId = session.Id, State = PhotoState.Ready };
        await store.Run(
            "fixture",
            async tx =>
            {
                await tx.Save(option, 0);
                await tx.Save(session, 0);
                await tx.Save(photo, 0);
                return true;
            }
        );
        object Input(Guid id, long revision) =>
            new
            {
                idempotencyKey = "reprice-check",
                lines = new[]
                {
                    new
                    {
                        photoId = id,
                        optionId = option.Id,
                        quantity = 1,
                        optionRevision = revision,
                    },
                },
            };
        Assert.Equal(
            HttpStatusCode.NotFound,
            (
                await client.PostAsJsonAsync("/api/client/print-requests", Input(Guid.NewGuid(), 1))
            ).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.Conflict,
            (
                await client.PostAsJsonAsync("/api/client/print-requests", Input(photo.Id, 0))
            ).StatusCode
        );
        Assert.Empty(await store.Run("fixture", tx => tx.List<PrintRequest>()));
    }
}
