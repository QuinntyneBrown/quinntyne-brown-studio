namespace QuinntyneBrownStudio.Application.Presentation;

/// <summary>
/// The fixed Contact page copy from the approved prototype (docs/mocks/assets/app.js, contactPage).
/// Only the heading and introduction are administrator-published.
/// </summary>
public static class ContactPageCopy
{
    public const string DefaultHeading = "Something beautiful starts with hello.";

    public const string DefaultIntroduction = "Tell us what you have in mind. We’ll find the right way to capture it.";

    public const string DefaultReplyNotice = "We usually reply within two working days.";

    public const string OnLocationTitle = "On location";

    public const string OnLocationBody =
        "Parks, venues, kitchens, and the end of your own street. Travel is itemized in your quote.";

    public static string ReplyNotice(string? replyNote) =>
        string.IsNullOrWhiteSpace(replyNote)
            ? DefaultReplyNotice
            : $"We usually reply {char.ToLowerInvariant(replyNote[0])}{replyNote[1..]}.";

    public static string PlacesHeading(int studios) =>
        studios switch
        {
            0 => "On location, anywhere you love.",
            1 => "One studio space. Any location you love.",
            2 => "Two studio spaces. Any location you love.",
            _ => $"{studios} studio spaces. Any location you love.",
        };

    public static ContactQuestion[] Questions(ContactAdvanceBooking? advance) =>
    [
        new(
            "How far ahead should we book?",
            "Weddings are usually reserved six to twelve months out. Portraits and headshots often have room within a few weeks."
                + (advance == null ? "" : $" Booking at least {advance.Days} days ahead also earns {advance.Percentage:0.##}% off.")
        ),
        new(
            "Do you travel?",
            "Yes. Toronto is home, and round-trip distance is itemized in your estimate so there are no surprises on the day."
        ),
        new(
            "How do we receive our photographs?",
            "Through a private online gallery, edited with care, with fine art prints available to order in a few sizes."
        ),
    ];
}
