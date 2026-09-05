using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Qbs.AcceptanceTests;

public sealed class PlatformAcceptanceTests
{
    [Theory]
    [InlineData("equipment")]
    [InlineData("vendors")]
    [InlineData("rates")]
    [InlineData("sessions")]
    [InlineData("public-galleries")]
    [InlineData("print-requests")]
    [InlineData("content")]
    public async Task AC_L2_003_01_Administration_rejects_anonymous_and_client(string resource)
    {
        using var factory = new StudioFactory();
        using var anonymous = await factory.Actor(null);
        using var client = await factory.Actor("Client");
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await anonymous.GetAsync($"/api/admin/{resource}")).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.Forbidden,
            (await client.GetAsync($"/api/admin/{resource}")).StatusCode
        );
    }

    [Fact]
    public async Task AC_L2_023_01_Equipment_persists_valid_changes_and_rejects_stale_or_negative_edits()
    {
        using var factory = new StudioFactory();
        using var admin = await factory.Actor();
        var response = await admin.PostAsJsonAsync(
            "/api/admin/equipment",
            new { name = "Camera", quantity = 2 }
        );
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var saved = await response.Content.ReadFromJsonAsync<JsonElement>();
        var id = saved.GetProperty("id").GetString();
        Assert.Equal(
            HttpStatusCode.OK,
            (
                await admin.PutAsJsonAsync(
                    $"/api/admin/equipment/{id}",
                    new
                    {
                        name = "Camera",
                        quantity = 3,
                        expectedVersion = 1,
                    }
                )
            ).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.Conflict,
            (
                await admin.PutAsJsonAsync(
                    $"/api/admin/equipment/{id}",
                    new
                    {
                        name = "Camera",
                        quantity = 4,
                        expectedVersion = 1,
                    }
                )
            ).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.BadRequest,
            (
                await admin.PutAsJsonAsync(
                    $"/api/admin/equipment/{id}",
                    new
                    {
                        name = "Camera",
                        quantity = -1,
                        expectedVersion = 2,
                    }
                )
            ).StatusCode
        );
        var list = await admin.GetFromJsonAsync<JsonElement[]>("/api/admin/equipment");
        Assert.Equal(3, Assert.Single(list!).GetProperty("quantity").GetInt32());
    }

    [Theory]
    [InlineData("MakeupArtist")]
    [InlineData("SecondShooter")]
    [InlineData("Assistant")]
    public async Task AC_L2_024_01_AC_L2_025_01_Vendor_roles_and_contact_validation(string role)
    {
        using var factory = new StudioFactory();
        using var admin = await factory.Actor();
        var valid = await admin.PostAsJsonAsync(
            "/api/admin/vendors",
            new
            {
                name = "Partner",
                email = "partner@example.test",
                roles = new[] { role },
            }
        );
        Assert.Equal(HttpStatusCode.Created, valid.StatusCode);
        Assert.Equal(
            HttpStatusCode.BadRequest,
            (
                await admin.PostAsJsonAsync(
                    "/api/admin/vendors",
                    new
                    {
                        name = "Partner",
                        email = "bad",
                        roles = new[] { role },
                    }
                )
            ).StatusCode
        );
    }

    [Fact]
    public async Task AC_L2_007_01_Promotion_publication_and_noneditable_notice()
    {
        using var factory = new StudioFactory();
        using var admin = await factory.Actor();
        using var visitor = await factory.Actor(null);
        await admin.PostAsJsonAsync(
            "/api/admin/promotions",
            new
            {
                title = "Draft",
                description = "Private",
                indicativePrice = "100.00",
                published = false,
            }
        );
        var published = await admin.PostAsJsonAsync(
            "/api/admin/promotions",
            new
            {
                title = "Portraits",
                description = "An offer",
                indicativePrice = "150.00",
                published = true,
            }
        );
        Assert.Equal(HttpStatusCode.Created, published.StatusCode);
        var offers = await visitor.GetFromJsonAsync<JsonElement[]>("/api/public/promotions");
        var offer = Assert.Single(offers!);
        Assert.Equal("Portraits", offer.GetProperty("title").GetString());
        Assert.Contains("consultation", offer.GetProperty("consultationNotice").GetString());
    }

    [Fact]
    public async Task AC_L2_020_01_Print_price_changes_have_one_public_source()
    {
        using var factory = new StudioFactory();
        using var admin = await factory.Actor();
        var response = await admin.PostAsJsonAsync(
            "/api/admin/print-options",
            new
            {
                name = "Portrait print",
                dimensions = "8 × 10",
                finish = "Matte",
                unitPrice = "25.50",
                enabled = true,
            }
        );
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var saved = await response.Content.ReadFromJsonAsync<JsonElement>();
        await admin.PutAsJsonAsync(
            $"/api/admin/print-options/{saved.GetProperty("id").GetString()}",
            new
            {
                name = "Portrait print",
                dimensions = "8 × 10",
                finish = "Matte",
                unitPrice = "30.00",
                enabled = true,
                expectedVersion = 1,
            }
        );
        var list = await admin.GetFromJsonAsync<JsonElement[]>("/api/public/print-options");
        Assert.Equal("30.00", Assert.Single(list!).GetProperty("unitPrice").GetString());
    }

    [Fact]
    public async Task AC_L2_012_02_Unconfigured_quote_is_unavailable_not_zero()
    {
        using var factory = new StudioFactory();
        using var visitor = await factory.Actor(null);
        var response = await visitor.PostAsJsonAsync(
            "/api/public/quotes/calculate",
            new
            {
                service = "Wedding",
                startsAt = "2027-06-01T10:00:00-04:00",
                endsAt = "2027-06-01T12:00:00-04:00",
                locations = new[]
                {
                    new
                    {
                        location = new
                        {
                            label = "Venue",
                            latitude = 43.6,
                            longitude = -79.4,
                        },
                        parkingAmount = "0",
                        studioHours = "0",
                    },
                },
                inputRevision = 1,
            }
        );
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.DoesNotContain("\"total\"", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task AC_L2_033_02_AC_L2_034_01_Client_isolation_and_empty_galleries()
    {
        using var factory = new StudioFactory();
        using var client = await factory.Actor("Client");
        Assert.Empty((await client.GetFromJsonAsync<JsonElement[]>("/api/client/galleries"))!);
        foreach (var resource in new[] { "galleries", "albums" })
            Assert.Equal(
                HttpStatusCode.NotFound,
                (await client.GetAsync($"/api/client/{resource}/{Guid.NewGuid()}")).StatusCode
            );
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await client.GetAsync($"/api/client/photos/{Guid.NewGuid()}/preview")).StatusCode
        );
    }
}
