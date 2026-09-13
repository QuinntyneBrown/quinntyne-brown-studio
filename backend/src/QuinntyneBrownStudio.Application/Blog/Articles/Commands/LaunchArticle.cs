namespace QuinntyneBrownStudio.Application.Blog.Articles.Commands;

/// <summary>The coming-soon article the relaunch gate publishes into an empty blog (OD-14).</summary>
public static class LaunchArticle
{
    public const string Title = "Coming soon";

    public const string Abstract =
        "Quinntyne Brown Studio is relaunching. The galleries are being rebuilt, and the studio is open for bookings in the meantime.";

    public const string Body = """
        # The studio is relaunching

        Quinntyne Brown Studio is being rebuilt from the ground up: a new site, new galleries, and a new way to plan a session.

        While the galleries fill up again, three things are already here.

        - **[About the studio](/about)**: who is behind the camera and how a session works.
        - **[Get in touch](/contact)**: questions, dates, and anything you want to talk through.
        - **[Find your quote](/quote)**: a live estimate for weddings, events, headshots, and family portraits.

        Portfolio, prints, and packages return with the relaunch. Until then, the blog is where the news lands first.
        """;
}
