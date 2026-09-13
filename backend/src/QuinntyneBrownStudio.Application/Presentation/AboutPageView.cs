namespace QuinntyneBrownStudio.Application.Presentation;

/// <summary>What the About page renders. <see cref="Fingerprint"/> names every record version the page read, for the ETag.</summary>
public sealed record AboutPageView(
    string Heading,
    string Introduction,
    bool IsDefaultCopy,
    AboutTeamMember[] Team,
    AboutHeroPhoto? HeroPhoto,
    string Fingerprint
);
