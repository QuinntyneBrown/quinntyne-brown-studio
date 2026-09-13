using QuinntyneBrownStudio.Domain.Entities;

namespace QuinntyneBrownStudio.Application.Presentation;

/// <summary>
/// What the Contact page renders. <see cref="Details"/> is null when no contact detail is configured,
/// and <see cref="Fingerprint"/> names every record version the page read, for the ETag.
/// </summary>
public sealed record ContactPageView(
    string Heading,
    string Introduction,
    string ReplyNotice,
    StudioDetails? Details,
    ContactStudio[] Studios,
    ContactAdvanceBooking? AdvanceBooking,
    ContactQuestion[] Questions,
    string Fingerprint
);
