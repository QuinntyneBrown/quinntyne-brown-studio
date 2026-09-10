using System.Security.Cryptography;

namespace QuinntyneBrownStudio.Api.Blog.Middleware;

public class SecurityHeadersMiddleware(RequestDelegate next)
{
    /// <summary>
    /// Key used to store the per-request CSP nonce in HttpContext.Items so that
    /// Razor Pages tag helpers can embed it into inline <style> blocks.
    /// </summary>
    public const string CspNonceKey = "CspNonce";

    public async Task InvokeAsync(HttpContext context)
    {
        // Generate a cryptographically-random per-request nonce (32 hex chars).
        // Hex encoding avoids HTML entity encoding issues that occur with base64's
        // +, /, = characters in nonce attributes.
        var nonceBytes = RandomNumberGenerator.GetBytes(16);
        var nonce = Convert.ToHexString(nonceBytes).ToLowerInvariant();

        // Store the nonce so Razor views/tag-helpers can embed it into <style> blocks.
        context.Items[CspNonceKey] = nonce;

        // Register a callback so headers are written just before the response body starts.
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;

            // Content-Security-Policy — nonce-based; eliminates 'unsafe-inline' for styles.
            // fonts.googleapis.com is allowed in style-src so the Google Fonts CSS stylesheet can
            // be applied (loaded via <link rel="preload" onload="this.rel='stylesheet'">).
            // fonts.gstatic.com is allowed in font-src so the actual .woff2 font binary files
            // (referenced by the Google Fonts stylesheet) can be downloaded.
            // report-uri (legacy) and report-to (modern) directives send CSP violations
            // to the /api/csp-report endpoint (Design 08, Section 3.3).
            headers["Content-Security-Policy"] =
                $"default-src 'self'; " +
                $"script-src 'self' 'nonce-{nonce}'; " +
                $"style-src 'self' 'nonce-{nonce}' https://fonts.googleapis.com; " +
                $"style-src-attr 'unsafe-inline'; " +
                $"font-src 'self' https://fonts.gstatic.com; " +
                $"img-src 'self' data:; " +
                $"frame-ancestors 'none'; " +
                $"object-src 'none'; " +
                $"base-uri 'self'; " +
                $"form-action 'self';";

            // Reporting-Endpoints header (modern browsers) — maps the "csp-endpoint" group
            // to the /api/csp-report URL (Design 08, Section 3.3).


            headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";

            headers["X-Content-Type-Options"] = "nosniff";
            headers["X-Frame-Options"] = "DENY";
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=()";

            // Remove the Server header to avoid information disclosure.
            headers.Remove("Server");

            return Task.CompletedTask;
        });

        await next(context);
    }
}
