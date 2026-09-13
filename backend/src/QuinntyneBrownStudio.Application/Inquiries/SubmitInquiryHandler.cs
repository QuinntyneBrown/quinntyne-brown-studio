using System.Text;
using MediatR;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Enums;

namespace QuinntyneBrownStudio.Application.Inquiries;

/// <summary>
/// Stores a validated inquiry with the next QB-IN reference and, when the studio has a configured
/// email address, queues the plain-text notification in the same unit of work.
/// </summary>
public sealed class SubmitInquiryHandler(IStudioStore store, IClock clock, IEmailQueue email)
    : IRequestHandler<SubmitInquiry, Inquiry>
{
    public const string ReferencePrefix = "QB-IN-";

    public Task<Inquiry> Handle(SubmitInquiry request, CancellationToken ct) =>
        store.Run(
            nameof(Inquiry),
            async tx =>
            {
                var existing = await tx.List<Inquiry>();
                var next =
                    existing
                        .Select(x =>
                            x.Reference.StartsWith(ReferencePrefix)
                            && int.TryParse(x.Reference.AsSpan(ReferencePrefix.Length), out var number)
                                ? number
                                : 0
                        )
                        .DefaultIfEmpty(1040)
                        .Max() + 1;
                var now = clock.UtcNow;
                var inquiry = new Inquiry
                {
                    Reference = ReferencePrefix + next,
                    Name = request.Name.Trim(),
                    Email = request.Email.Trim(),
                    Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
                    Interest = Enum.Parse<ServiceKind>(request.Interest),
                    Message = request.Message.Trim(),
                    ConsentAt = now,
                    SubmittedAt = now,
                };
                await tx.Save(inquiry, 0);
                var details = await tx.Get<StudioDetails>(AdminCatalog.ConfigurationId);
                if (!string.IsNullOrWhiteSpace(details?.Email))
                    await email.Queue(
                        tx,
                        details.Email,
                        $"New inquiry {inquiry.Reference} from {inquiry.Name}",
                        Notification(inquiry),
                        inquiry.Id,
                        ct
                    );
                return inquiry;
            },
            ct
        );

    public static string InterestLabel(ServiceKind interest) =>
        interest switch
        {
            ServiceKind.Headshot => "Headshots",
            ServiceKind.FamilyPortrait => "Family portraits",
            _ => interest.ToString(),
        };

    private static string Notification(Inquiry inquiry)
    {
        var text = new StringBuilder();
        text.AppendLine("A new inquiry arrived from the contact page.");
        text.AppendLine();
        text.AppendLine($"Reference: {inquiry.Reference}");
        text.AppendLine($"Name: {inquiry.Name}");
        text.AppendLine($"Email: {inquiry.Email}");
        text.AppendLine($"Phone: {inquiry.Phone ?? "Not provided"}");
        text.AppendLine($"Interest: {InterestLabel(inquiry.Interest)}");
        text.AppendLine($"Received: {inquiry.SubmittedAt:yyyy-MM-dd HH:mm} UTC");
        text.AppendLine();
        text.AppendLine("Message:");
        text.AppendLine(inquiry.Message);
        text.AppendLine();
        text.AppendLine("Reply from your own mailbox. The inquiry is kept in the studio administration inbox.");
        return text.ToString();
    }
}
