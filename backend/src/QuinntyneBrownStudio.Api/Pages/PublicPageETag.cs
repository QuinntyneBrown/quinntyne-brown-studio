using System.Security.Cryptography;
using System.Text;

namespace QuinntyneBrownStudio.Api.Pages;

/// <summary>
/// Weak ETags for the server-rendered marketing pages: a hash of the page name and the
/// versions of every record the page read, so a browser or the gateway can revalidate.
/// </summary>
public static class PublicPageETag
{
    public static string Compute(string page, string fingerprint)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{page}-{fingerprint}"));
        return $"W/\"{Convert.ToHexString(hash[..8]).ToLowerInvariant()}\"";
    }
}
