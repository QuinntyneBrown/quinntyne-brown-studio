namespace QuinntyneBrownStudio.Application.Blog.Services;

public interface IETagGenerator
{
    /// <summary>
    /// Builds the weak ETag string for an article page in the form
    /// <c>W/"article-{articleId}-v{version}"</c>.
    /// </summary>
    string Generate(Guid articleId, int version);

    /// <summary>
    /// Builds the weak ETag string for the about page in the form
    /// <c>W/"about:{version}"</c>.
    /// </summary>
    string GenerateAbout(int version);

    /// <summary>
    /// Returns <see langword="true"/> when the client-supplied <paramref name="ifNoneMatch"/>
    /// header value matches the <paramref name="etag"/>, indicating the client already holds
    /// the current representation and a 304 Not Modified response can be issued.
    /// </summary>
    bool IsMatch(string etag, string? ifNoneMatch);
}
