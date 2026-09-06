using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Enums;

namespace QuinntyneBrownStudio.AcceptanceTests;

public sealed class WorkflowAcceptanceTests
{
    // Wedding fixture: 200 + 24 + 80 + 60 + 45 + 10 + 75 = 494; less 12% = 434.72.
    [Theory]
    [InlineData("Wedding", "434.72")]
    [InlineData("Event", "610.72")]
    [InlineData("Headshot", "786.72")]
    [InlineData("FamilyPortrait", "962.72")]
    public async Task AC_L2_008_01_AC_L2_009_01_AC_L2_010_01_AC_L2_013_01_Quotes_use_saved_decimal_rates(
        string service,
        string expected
    )
    {
        using var factory = new StudioFactory();
        using var admin = await factory.Actor();
        var studio = await Create(
            admin,
            "studios",
            new
            {
                name = "Studio",
                resolvedAddress = new
                {
                    label = "Studio base",
                    latitude = 43.6,
                    longitude = -79.4,
                },
                hourlyFee = "50",
                enabled = true,
                isBase = true,
            }
        );
        (
            await admin.PutAsJsonAsync(
                "/api/admin/rates",
                new
                {
                    serviceRates = new
                    {
                        Wedding = "100",
                        Event = "200",
                        Headshot = "300",
                        FamilyPortrait = "400",
                    },
                    costRates = new
                    {
                        travel = "2",
                        assistant = "40",
                        equipment = "30",
                        lunch = "15",
                    },
                    expectedVersion = 0,
                }
            )
        ).EnsureSuccessStatusCode();
        (
            await admin.PutAsJsonAsync(
                "/api/admin/discounts",
                new
                {
                    advanceRule = new
                    {
                        enabled = false,
                        percentage = "0",
                        threshold = 90,
                    },
                    weekdayRule = new
                    {
                        enabled = false,
                        percentage = "0",
                        weekdays = Array.Empty<string>(),
                    },
                    codeRules = new[]
                    {
                        new
                        {
                            code = "STUDIO12",
                            enabled = true,
                            percentage = "12",
                        },
                    },
                    expectedVersion = 0,
                }
            )
        ).EnsureSuccessStatusCode();
        using var visitor = await factory.Actor(null);
        var response = await visitor.PostAsJsonAsync(
            "/api/public/quotes/calculate",
            new
            {
                service,
                startsAt = "2027-06-01T10:00:00-04:00",
                endsAt = "2027-06-01T12:00:00-04:00",
                locations = new[]
                {
                    new
                    {
                        location = new
                        {
                            label = "Venue",
                            latitude = 43.7,
                            longitude = -79.3,
                        },
                        parkingAmount = "10",
                        studioId = studio.GetProperty("id").GetGuid(),
                        studioHours = "1.5",
                    },
                },
                assistantCount = 1,
                equipmentUnits = 2,
                lunchCount = 3,
                code = " studio12 ",
                inputRevision = 7,
            }
        );
        response.EnsureSuccessStatusCode();
        var quote = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(expected, quote.GetProperty("total").GetProperty("amount").GetString());
        Assert.Equal(7, quote.GetProperty("inputRevision").GetInt32());
        Assert.Equal("Code", quote.GetProperty("discount").GetProperty("kind").GetString());
        Assert.False(quote.GetProperty("availability").GetProperty("available").GetBoolean());
    }

    [Fact]
    public async Task AC_L2_021_01_AC_L2_022_01_AC_L2_058_01_Schedules_reject_overlapping_commitments()
    {
        using var factory = new StudioFactory();
        using var admin = await factory.Actor();
        var person = await Create(
            admin,
            "photographers",
            new { name = "Photographer", active = true }
        );
        var id = person.GetProperty("id").GetGuid();
        (
            await admin.PutAsJsonAsync(
                $"/api/admin/photographers/{id}/schedule",
                new
                {
                    workingWindows = new[]
                    {
                        new
                        {
                            startsAt = "2027-06-01T09:00:00-04:00",
                            endsAt = "2027-06-01T17:00:00-04:00",
                        },
                    },
                    unavailableWindows = Array.Empty<object>(),
                    buffers = new { before = 30, after = 30 },
                    expectedVersion = 0,
                }
            )
        ).EnsureSuccessStatusCode();
        await Create(
            admin,
            "sessions",
            new
            {
                name = "First",
                service = "Wedding",
                startsAt = "2027-06-01T10:00:00-04:00",
                endsAt = "2027-06-01T12:00:00-04:00",
                photographerId = id,
            }
        );
        var conflict = await admin.PostAsJsonAsync(
            "/api/admin/sessions",
            new
            {
                name = "Conflict",
                service = "Wedding",
                startsAt = "2027-06-01T12:00:00-04:00",
                endsAt = "2027-06-01T13:00:00-04:00",
                photographerId = id,
            }
        );
        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
        var adjacent = await admin.PostAsJsonAsync(
            "/api/public/availability",
            new
            {
                startsAt = "2027-06-01T13:00:00-04:00",
                endsAt = "2027-06-01T14:00:00-04:00",
                photographerId = id,
            }
        );
        adjacent.EnsureSuccessStatusCode();
        Assert.True(
            (await adjacent.Content.ReadFromJsonAsync<JsonElement>())
                .GetProperty("available")
                .GetBoolean()
        );
        Assert.Single((await admin.GetFromJsonAsync<JsonElement[]>("/api/admin/sessions"))!);
    }

    [Fact]
    public async Task AC_L2_036_01_AC_L2_064_01_Print_snapshots_are_idempotent_and_immutable()
    {
        using var factory = new StudioFactory();
        var clientId = Guid.NewGuid();
        var photo = await SeedPhoto(factory, clientId);
        using var admin = await factory.Actor();
        using var client = await factory.Actor("Client", clientId.ToString());
        var option = await Create(
            admin,
            "print-options",
            new
            {
                name = "Portrait",
                dimensions = "8x10",
                finish = "Matte",
                unitPrice = "25.50",
                enabled = true,
            }
        );
        var body = new
        {
            idempotencyKey = "submission-one",
            lines = new[]
            {
                new
                {
                    photoId = photo.Id,
                    optionId = option.GetProperty("id").GetGuid(),
                    optionRevision = 1,
                    quantity = 2,
                },
            },
        };
        var first = await client.PostAsJsonAsync("/api/client/print-requests", body);
        first.EnsureSuccessStatusCode();
        var a = await first.Content.ReadFromJsonAsync<JsonElement>();
        var again = await client.PostAsJsonAsync("/api/client/print-requests", body);
        again.EnsureSuccessStatusCode();
        Assert.Equal(
            a.GetProperty("id").GetGuid(),
            (await again.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid()
        );
        Assert.Equal("51.00", a.GetProperty("total").GetString());
        Assert.Single((await admin.GetFromJsonAsync<JsonElement[]>("/api/admin/print-requests"))!);
        var changed = await client.PostAsJsonAsync(
            "/api/client/print-requests",
            new
            {
                idempotencyKey = "submission-one",
                lines = new[]
                {
                    new
                    {
                        photoId = photo.Id,
                        optionId = option.GetProperty("id").GetGuid(),
                        optionRevision = 1,
                        quantity = 3,
                    },
                },
            }
        );
        Assert.Equal(HttpStatusCode.Conflict, changed.StatusCode);
    }

    [Fact]
    public async Task AC_L2_037_01_AC_L2_065_01_Albums_preserve_order_and_revoked_placeholders()
    {
        using var factory = new StudioFactory();
        var clientId = Guid.NewGuid();
        var photo = await SeedPhoto(factory, clientId);
        using var client = await factory.Actor("Client", clientId.ToString());
        var create = await client.PostAsJsonAsync(
            "/api/client/albums",
            new { name = "Memories", photoIds = new[] { photo.Id } }
        );
        create.EnsureSuccessStatusCode();
        var album = await create.Content.ReadFromJsonAsync<JsonElement>();
        var store = factory.Services.GetRequiredService<IStudioStore>();
        await store.Run(
            "fixture",
            async tx =>
            {
                var session = (await tx.Get<Session>(photo.SessionId))!;
                session.ClientIds = [];
                await tx.Save(session, session.Version);
                return true;
            }
        );
        var view = await client.GetFromJsonAsync<JsonElement>(
            $"/api/client/albums/{album.GetProperty("id").GetGuid()}"
        );
        var item = Assert.Single(view.GetProperty("photos").EnumerateArray());
        Assert.False(item.GetProperty("available").GetBoolean());
        Assert.False(item.TryGetProperty("url", out _));
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await client.GetAsync($"/api/client/photos/{photo.Id}/preview")).StatusCode
        );
    }

    [Fact]
    public async Task AC_L2_061_01_Deletion_requires_current_impact_and_blocks_publication()
    {
        using var factory = new StudioFactory();
        var photo = await SeedPhoto(factory, Guid.NewGuid());
        using var admin = await factory.Actor();
        var impact = await admin.GetFromJsonAsync<JsonElement>(
            $"/api/admin/sessions/{photo.SessionId}/retention"
        );
        await Create(
            admin,
            "public-galleries",
            new
            {
                title = "Selected work",
                slug = "selected-work",
                photoIds = new[] { photo.Id },
                published = true,
            }
        );
        var stale = await admin.PostAsJsonAsync(
            $"/api/admin/sessions/{photo.SessionId}/photo-deletion",
            new
            {
                impactRevision = impact.GetProperty("impactRevision").GetString(),
                confirm = true,
            }
        );
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        var current = await admin.GetFromJsonAsync<JsonElement>(
            $"/api/admin/sessions/{photo.SessionId}/retention"
        );
        Assert.Equal(1, current.GetProperty("publishedReferences").GetInt32());
        var blocked = await admin.PostAsJsonAsync(
            $"/api/admin/sessions/{photo.SessionId}/photo-deletion",
            new
            {
                impactRevision = current.GetProperty("impactRevision").GetString(),
                confirm = true,
            }
        );
        Assert.Equal(HttpStatusCode.Conflict, blocked.StatusCode);
    }

    [Fact]
    public async Task AC_L2_026_01_AC_L2_028_01_Upload_manifest_rejects_invalid_files_and_oversized_batches()
    {
        using var factory = new StudioFactory();
        using var admin = await factory.Actor();
        var session = await Create(
            admin,
            "sessions",
            new
            {
                name = "Portraits",
                service = "Headshot",
                startsAt = "2027-06-01T10:00:00-04:00",
                endsAt = "2027-06-01T12:00:00-04:00",
            }
        );
        var id = session.GetProperty("id").GetGuid();
        var files = new[]
        {
            new
            {
                clientFileId = "a",
                name = "photo.jpg",
                size = 1000,
                sha256 = new string('a', 64),
            },
            new
            {
                clientFileId = "b",
                name = "bad.exe",
                size = 1000,
                sha256 = new string('a', 64),
            },
        };
        var response = await admin.PostAsJsonAsync(
            $"/api/admin/sessions/{id}/uploads",
            new { files }
        );
        response.EnsureSuccessStatusCode();
        var batch = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(
            JsonValueKind.Null,
            batch.GetProperty("files")[0].GetProperty("rejection").ValueKind
        );
        Assert.NotNull(batch.GetProperty("files")[1].GetProperty("rejection").GetString());
        Assert.Single(batch.GetProperty("grants").EnumerateObject());
        var tooMany = await admin.PostAsJsonAsync(
            $"/api/admin/sessions/{id}/uploads",
            new
            {
                files = Enumerable
                    .Range(0, 1001)
                    .Select(i => new
                    {
                        clientFileId = i.ToString(),
                        name = "a.jpg",
                        size = 1000,
                        sha256 = new string('a', 64),
                    }),
            }
        );
        Assert.Equal(HttpStatusCode.BadRequest, tooMany.StatusCode);
    }

    [Fact]
    public async Task AC_L2_062_01_Mutations_require_antiforgery_even_for_authenticated_administrators()
    {
        using var factory = new StudioFactory();
        using var admin = await factory.Actor();
        admin.DefaultRequestHeaders.Remove("X-XSRF-TOKEN");
        Assert.Equal(
            HttpStatusCode.BadRequest,
            (
                await admin.PostAsJsonAsync(
                    "/api/admin/equipment",
                    new { name = "Camera", quantity = 1 }
                )
            ).StatusCode
        );
        Assert.Empty((await admin.GetFromJsonAsync<JsonElement[]>("/api/admin/equipment"))!);
    }

    private static async Task<JsonElement> Create(HttpClient client, string resource, object body)
    {
        var response = await client.PostAsJsonAsync("/api/admin/" + resource, body);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    private static async Task<SessionPhoto> SeedPhoto(StudioFactory factory, Guid clientId)
    {
        var store = factory.Services.GetRequiredService<IStudioStore>();
        return await store.Run(
            "fixture",
            async tx =>
            {
                var session = new Session
                {
                    Name = "Private session",
                    ClientIds = [clientId],
                    ExpiresAt = DateTimeOffset.UtcNow.AddMonths(12),
                    ExpiryRevision = 1,
                };
                await tx.Save(session, 0);
                var photo = new SessionPhoto
                {
                    SessionId = session.Id,
                    State = PhotoState.Ready,
                    Name = "portrait.jpg",
                    PreviewKey = "previews/fixture.jpg",
                };
                await tx.Save(photo, 0);
                return photo;
            }
        );
    }
}
