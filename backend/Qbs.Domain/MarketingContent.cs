namespace Qbs.Domain;

public sealed class MarketingContent : Entity
{
    public string PageKey { get; set; } = "";
    public string Heading { get; set; } = "";
    public string Body { get; set; } = "";
    public bool Publish { get; set; }
    public string? PublishedHeading { get; set; }
    public string? PublishedBody { get; set; }
}
