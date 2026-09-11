namespace QuinntyneBrownStudio.Application.Blog.Models;

public static class PaginationHelper
{
    /// <summary>
    /// Populates <see cref="PagedResponse{T}.PreviousPageUrl"/> and
    /// <see cref="PagedResponse{T}.NextPageUrl"/> using the supplied base URL
    /// and request path.
    /// </summary>
    /// <param name="response">The paged response to enrich.</param>
    /// <param name="baseUrl">Configured site base URL (e.g. "https://example.com"). Trailing slashes are trimmed.</param>
    /// <param name="requestPath">The current request path (e.g. "/blog/api/articles").</param>
    public static void SetNavigationUrls<T>(PagedResponse<T> response, string baseUrl, string requestPath, IReadOnlyDictionary<string, string>? parameters = null)
    {
        if (response.PageSize <= 0)
            return;

        var trimmedBase = baseUrl.TrimEnd('/');

        var query = parameters == null ? "" : string.Concat(parameters.Where(p => p.Key != "page" && p.Key != "pageSize").Select(p => "&" + Uri.EscapeDataString(p.Key) + "=" + Uri.EscapeDataString(p.Value)));
        if (response.HasPreviousPage)
            response.PreviousPageUrl = $"{trimmedBase}{requestPath}?page={response.Page - 1}&pageSize={response.PageSize}{query}";

        if (response.HasNextPage)
            response.NextPageUrl = $"{trimmedBase}{requestPath}?page={response.Page + 1}&pageSize={response.PageSize}{query}";
    }
}
