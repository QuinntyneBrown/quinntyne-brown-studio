namespace QuinntyneBrownStudio.Application.Presentation;

/// <summary>
/// The relaunch gate (OD-14). While <see cref="ComingSoon"/> is on, the client-rendered marketing
/// pages stay behind the blog for anyone who is not signed in. Bound from the <c>Launch</c> section.
/// </summary>
public sealed class LaunchOptions
{
    public bool ComingSoon { get; set; }
}
