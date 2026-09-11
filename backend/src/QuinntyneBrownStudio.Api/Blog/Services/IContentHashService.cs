namespace QuinntyneBrownStudio.Api.Blog.Services;

public interface IContentHashService
{
    /// <summary>
    /// Returns a versioned path for the given static asset path.
    /// Example: "/blog/css/app.css" → "/blog/css/app.a1b2c3d4.css"
    /// If the file does not exist, returns the original path unchanged.
    /// </summary>
    string GetHashedPath(string path);

    /// <summary>
    /// Resolves a hashed path back to the original file path.
    /// Example: "/blog/css/app.a1b2c3d4.css" → "/blog/css/app.css"
    /// Returns null if the path does not match the hash pattern.
    /// </summary>
    string? ResolveHashedPath(string hashedPath);
}
