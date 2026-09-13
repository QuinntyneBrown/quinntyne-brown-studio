using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Enums;
using QuinntyneBrownStudio.Infrastructure.Adapters;
using QuinntyneBrownStudio.Infrastructure.Processing;

namespace QuinntyneBrownStudio.AcceptanceTests;

// Acceptance tests: AC-L2-073-01 through AC-L2-073-05 (a plain form post is the script-free path),
// AC-L2-074-02 and AC-L2-074-03. Each rate-limited case uses its own factory: rejected posts
// consume permits too, and the window allows five per client address.
public sealed class ContactInquiryAcceptanceTests
{
    [Fact]
    public async Task AC_L2_073_01_A_valid_message_is_stored_with_a_reference_and_confirmed_with_an_empty_form()
    {
        await using var factory = new StudioFactory();
        using var visitor = Visitor(factory);
        var response = await Post(visitor, Valid());
        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        Assert.Equal("/contact?sent=QB-IN-1041", response.Headers.Location?.OriginalString);

        var confirmation = await visitor.GetStringAsync("/contact?sent=QB-IN-1041");
        Assert.Contains("Reference QB-IN-1041", confirmation);
        Assert.DoesNotContain("value=\"Priya Raman\"", confirmation);
        Assert.DoesNotContain("A small September wedding", confirmation);

        var inquiry = Assert.Single(await Inquiries(factory));
        Assert.Equal("QB-IN-1041", inquiry.Reference);
        Assert.Equal("Priya Raman", inquiry.Name);
        Assert.Equal("priya@example.test", inquiry.Email);
        Assert.Equal("416-555-0166", inquiry.Phone);
        Assert.Equal(ServiceKind.Wedding, inquiry.Interest);
        Assert.Equal("A small September wedding, about forty guests.", inquiry.Message);
        Assert.Equal("Submitted", inquiry.State);
        Assert.True(inquiry.SubmittedAt > DateTimeOffset.UtcNow.AddMinutes(-1));
        Assert.True(inquiry.ConsentAt > DateTimeOffset.UtcNow.AddMinutes(-1));
        Assert.Null(inquiry.ReviewedBy);

        var second = await Post(visitor, Valid(email: "second@example.test"));
        Assert.Equal("/contact?sent=QB-IN-1042", second.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData("name", "", "error-name")]
    [InlineData("email", "", "error-email")]
    [InlineData("email", "not-an-address", "error-email")]
    [InlineData("message", "", "error-message")]
    [InlineData("consent", "", "error-consent")]
    [InlineData("interest", "Boudoir", "error-interest")]
    [InlineData("name", "long", "error-name")]
    [InlineData("email", "long", "error-email")]
    [InlineData("phone", "long", "error-phone")]
    [InlineData("message", "long", "error-message")]
    public async Task AC_L2_073_02_An_invalid_message_is_not_stored_and_re_renders_with_the_error_beside_its_field(
        string field, string value, string errorId)
    {
        await using var factory = new StudioFactory();
        using var visitor = Visitor(factory);
        var fields = Valid();
        fields[field] = value switch
        {
            "long" => field switch
            {
                "name" => new string('n', 201),
                "email" => new string('e', 250) + "@x.io",
                "phone" => new string('7', 51),
                _ => new string('m', 4001),
            },
            _ => value,
        };
        if (field == "consent" && value == "")
            fields.Remove("consent");

        var response = await Post(visitor, fields);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains($"id=\"{errorId}\"", html);
        Assert.Contains($"aria-describedby=\"{errorId}\"", html);
        if (field != "name")
            Assert.Contains("value=\"Priya Raman\"", html);
        if (field != "message")
            Assert.Contains("A small September wedding, about forty guests.</textarea>", html);
        if (field != "phone")
            Assert.Contains("value=\"416-555-0166\"", html);
        if (field != "interest")
            Assert.Contains("value=\"Wedding\" selected", html);
        Assert.Empty(await Inquiries(factory));
    }

    [Fact]
    public async Task AC_L2_073_02_Several_empty_fields_are_all_reported_at_once()
    {
        await using var factory = new StudioFactory();
        using var visitor = Visitor(factory);
        var fields = Valid();
        fields["name"] = "";
        fields["email"] = "";
        fields["message"] = "";
        fields.Remove("consent");
        var html = await (await Post(visitor, fields)).Content.ReadAsStringAsync();
        foreach (var id in new[] { "error-name", "error-email", "error-message", "error-consent" })
            Assert.Contains($"id=\"{id}\"", html);
        Assert.Contains("value=\"416-555-0166\"", html);
        Assert.Empty(await Inquiries(factory));
    }

    [Fact]
    public async Task AC_L2_073_03_A_store_outage_keeps_the_entries_and_a_retry_stores_exactly_one_inquiry()
    {
        await using var factory = new StudioFactory
        {
            ConfigurePersistence = services =>
            {
                services.RemoveAll<IStudioStore>();
                services.AddSingleton<IStudioStore>(new FailingOnceStudioStore(new MemoryStudioStore(), "Inquiry"));
            },
        };
        using var visitor = Visitor(factory);
        var first = await Post(visitor, Valid());
        var html = await first.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Contains("could not be sent", html);
        Assert.Contains("value=\"Priya Raman\"", html);
        Assert.Contains("A small September wedding, about forty guests.</textarea>", html);
        Assert.Empty(await Inquiries(factory));

        var retry = await Post(visitor, Valid());
        Assert.Equal(HttpStatusCode.SeeOther, retry.StatusCode);
        Assert.Single(await Inquiries(factory));
    }

    [Fact]
    public async Task AC_L2_073_04_A_post_without_an_antiforgery_token_stores_nothing()
    {
        await using var factory = new StudioFactory();
        using var visitor = Visitor(factory);
        var response = await visitor.PostAsync("/contact", new FormUrlEncodedContent(Valid()));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(await Inquiries(factory));
    }

    [Fact]
    public async Task AC_L2_073_04_A_filled_hidden_field_stores_nothing_and_queues_no_email()
    {
        await using var factory = new StudioFactory();
        using var admin = await factory.Actor();
        await ConfigureStudioEmail(admin, "studio@example.test");
        using var visitor = Visitor(factory);
        var fields = Valid();
        fields["website"] = "https://spam.example";
        var response = await Post(visitor, fields);
        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        Assert.Equal("/contact", response.Headers.Location?.OriginalString);
        Assert.Empty(await Inquiries(factory));
        Assert.Empty(await EmailJobs(factory));
    }

    [Fact]
    public async Task AC_L2_073_04_The_sixth_submission_from_one_address_within_ten_minutes_is_rejected()
    {
        await using var factory = new StudioFactory();
        using var visitor = Visitor(factory);
        for (var attempt = 1; attempt <= 5; attempt++)
            Assert.Equal(HttpStatusCode.SeeOther, (await Post(visitor, Valid(email: $"visitor{attempt}@example.test"))).StatusCode);
        var sixth = await Post(visitor, Valid(email: "visitor6@example.test"));
        Assert.Equal(HttpStatusCode.TooManyRequests, sixth.StatusCode);
        Assert.Equal(5, (await Inquiries(factory)).Length);
    }

    [Fact]
    public async Task AC_L2_074_02_A_stored_inquiry_queues_one_notification_that_a_repeated_delivery_does_not_duplicate()
    {
        await using var factory = new StudioFactory();
        using var admin = await factory.Actor();
        await ConfigureStudioEmail(admin, "studio@example.test");
        using var visitor = Visitor(factory);
        Assert.Equal(HttpStatusCode.SeeOther, (await Post(visitor, Valid())).StatusCode);

        var inquiry = Assert.Single(await Inquiries(factory));
        var job = Assert.Single(await EmailJobs(factory));
        Assert.Equal(inquiry.Id, job.ResourceId);
        var payload = JsonSerializer.Deserialize<JsonElement>(
            factory.Services.GetRequiredService<IDataProtectionProvider>().CreateProtector("qbs-email-v1").Unprotect(job.Payload)
        );
        Assert.Equal("studio@example.test", payload.GetProperty("recipient").GetString());
        var body = payload.GetProperty("body").GetString()!;
        foreach (var expected in new[] { "QB-IN-1041", "Priya Raman", "priya@example.test", "416-555-0166", "Wedding", "A small September wedding, about forty guests." })
            Assert.Contains(expected, body);
        Assert.DoesNotContain("<", body);

        await using var scope = factory.Services.CreateAsyncScope();
        var processor = scope.ServiceProvider.GetRequiredService<JobProcessor>();
        var email = (ControlledEmail)scope.ServiceProvider.GetRequiredService<IEmailSender>();
        await processor.Process(job.Id, CancellationToken.None);
        Assert.Single(email.Messages);

        // A duplicate relay re-queues the same job; the provider sees the same deduplication id.
        var store = scope.ServiceProvider.GetRequiredService<IStudioStore>();
        await store.Run("test", async tx =>
        {
            var again = (await tx.Get<BackgroundJob>(job.Id))!;
            again.State = "Queued";
            again.AvailableAt = DateTimeOffset.UtcNow.AddMinutes(-1);
            again.LeaseUntil = null;
            await tx.Save(again, again.Version);
            return true;
        });
        await processor.Process(job.Id, CancellationToken.None);
        Assert.Single(email.Messages);
    }

    [Fact]
    public async Task AC_L2_074_03_Without_a_configured_address_the_inquiry_is_stored_and_no_email_is_queued()
    {
        await using var factory = new StudioFactory();
        using var visitor = Visitor(factory);
        var response = await Post(visitor, Valid());
        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        Assert.Single(await Inquiries(factory));
        Assert.Empty(await EmailJobs(factory));
    }

    [Fact]
    public async Task AC_L2_074_03_A_failed_delivery_is_retried_by_the_worker_without_losing_the_inquiry()
    {
        var sender = new ThrowingEmailSender();
        await using var factory = new StudioFactory
        {
            ConfigurePersistence = services =>
            {
                services.RemoveAll<IEmailSender>();
                services.AddSingleton<IEmailSender>(sender);
            },
        };
        using var admin = await factory.Actor();
        await ConfigureStudioEmail(admin, "studio@example.test");
        using var visitor = Visitor(factory);
        Assert.Equal(HttpStatusCode.SeeOther, (await Post(visitor, Valid())).StatusCode);
        var job = Assert.Single(await EmailJobs(factory));

        await using var scope = factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<JobProcessor>().Process(job.Id, CancellationToken.None);
        Assert.Equal(1, sender.Attempts);
        var retried = Assert.Single(await EmailJobs(factory));
        Assert.Equal("Queued", retried.State);
        Assert.Equal(1, retried.Attempt);
        Assert.True(retried.AvailableAt > DateTimeOffset.UtcNow);
        var inquiry = Assert.Single(await Inquiries(factory));
        Assert.Equal("Submitted", inquiry.State);
        Assert.Equal("Priya Raman", inquiry.Name);
    }

    private static HttpClient Visitor(StudioFactory factory) =>
        factory.CreateClient(new() { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });

    private static Dictionary<string, string> Valid(string email = "priya@example.test") =>
        new()
        {
            ["name"] = "Priya Raman",
            ["email"] = email,
            ["phone"] = "416-555-0166",
            ["interest"] = "Wedding",
            ["message"] = "A small September wedding, about forty guests.",
            ["consent"] = "true",
            ["website"] = "",
        };

    /// <summary>Posts the form the way a browser without script does: the antiforgery token comes from the rendered page.</summary>
    private static async Task<HttpResponseMessage> Post(HttpClient visitor, Dictionary<string, string> fields)
    {
        var page = await visitor.GetStringAsync("/contact");
        var token = Regex.Match(page, "__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;
        Assert.False(string.IsNullOrEmpty(token), "The contact form carries an antiforgery token.");
        fields["__RequestVerificationToken"] = token;
        return await visitor.PostAsync("/contact", new FormUrlEncodedContent(fields));
    }

    private static async Task ConfigureStudioEmail(HttpClient admin, string email) =>
        (await admin.PutAsJsonAsync("/api/admin/studio-details", new { email, expectedVersion = 0 })).EnsureSuccessStatusCode();

    private static async Task<Inquiry[]> Inquiries(StudioFactory factory)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IStudioStore>().Run("test", tx => tx.List<Inquiry>());
    }

    private static async Task<BackgroundJob[]> EmailJobs(StudioFactory factory)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var jobs = await scope.ServiceProvider.GetRequiredService<IStudioStore>().Run("test", tx => tx.List<BackgroundJob>());
        return jobs.Where(x => x.Kind == "Email").ToArray();
    }
}
