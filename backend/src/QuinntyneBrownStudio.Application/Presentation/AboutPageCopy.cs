namespace QuinntyneBrownStudio.Application.Presentation;

/// <summary>
/// The fixed About page copy from the approved prototype (docs/mocks/assets/app.js, aboutPage).
/// It ships with the release; only the heading and introduction are administrator-published.
/// </summary>
public static class AboutPageCopy
{
    public const string DefaultHeading = "Photographs that feel like you.";

    public const string DefaultIntroduction =
        "A small Toronto studio photographing weddings, events, headshots, and family portraits the way they actually feel: unhurried, honest, and a little bit wild.";

    public const string StoryHeading = "A borrowed camera, a friend’s wedding, and a lot of listening.";

    public static readonly string[] Story =
    [
        "Quinntyne Brown Studio began in 2018 with one camera and one favour: photographing a friend’s small backyard wedding. The photographs that mattered most weren’t the posed ones. They were the grandmother laughing mid-story, and the two of them alone for a moment on the back steps.",
        "That afternoon became the way we work. We plan carefully so that the day itself can be unscripted. We pay attention to light, timing, and the people in the room, and we let the rest happen.",
        "Today the studio photographs weddings, events, headshots, and family portraits across Toronto and wherever the story travels. The camera is better now. The listening is the same.",
    ];

    public static readonly (string Title, string Body)[] Principles =
    [
        ("Less posing. More being.", "We give gentle direction and then get out of the way. The photographs people keep are the ones where they forgot we were there."),
        ("Planning that feels like a conversation.", "Every session starts with a chat about what matters to you. Timelines, locations, and the small details are settled together, not handed down."),
        ("Honest pricing, from the first estimate.", "Photography, travel, and extras are itemized before you commit. Discounts apply automatically when you qualify, and nothing hides in the fine print."),
    ];

    public static readonly (string Title, string Body)[] Steps =
    [
        ("A conversation", "We talk through the day, the people, and the feeling you want to keep. You leave with a clear estimate and a plan."),
        ("The session", "Unhurried, relaxed, and on your terms. We guide when it helps and step back when it doesn’t."),
        ("Your gallery", "A carefully edited private gallery within a few weeks, with fine art prints available whenever you’re ready."),
    ];

    public const string TeamNote =
        "Alongside the studio is a trusted circle of second shooters, makeup artists, and assistants who join us when a day calls for more hands.";

    public const string FounderRole = "Founder & lead photographer";

    public const string PhotographerRole = "Photographer";
}
