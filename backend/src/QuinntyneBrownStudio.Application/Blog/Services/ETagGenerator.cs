namespace QuinntyneBrownStudio.Application.Blog.Services;

public class ETagGenerator : IETagGenerator
{
    /// <inheritdoc />
    public string Generate(Guid articleId, int version)
        => $"W/\"article-{articleId}-v{version}\"";

    /// <inheritdoc />
    public string GenerateAbout(int version)
        => $"W/\"about:{version}\"";

    /// <inheritdoc />
    public bool IsMatch(string etag, string? ifNoneMatch)
    {
        if (string.IsNullOrEmpty(ifNoneMatch))
            return false;

        // The If-None-Match header may contain a comma-separated list of ETags or "*".
        if (ifNoneMatch.Trim() == "*")
            return true;

        foreach (var candidate in ifNoneMatch.Split(','))
        {
            if (candidate.Trim().Equals(etag, StringComparison.Ordinal))
                return true;
        }

        return false;
    }
}
