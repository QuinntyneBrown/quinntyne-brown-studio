using Microsoft.AspNetCore.Razor.TagHelpers;

namespace QuinntyneBrownStudio.Api.Blog.TagHelpers;

[HtmlTargetElement("img", Attributes = "responsive")]
public class ResponsiveImageTagHelper : TagHelper
{
    private static readonly int[] DefaultBreakpoints = [320, 640, 960, 1280, 1920];
    private static readonly int[] CardBreakpoints = [320, 640, 960];

    [HtmlAttributeName("src")]
    public string Src { get; set; } = "";

    [HtmlAttributeName("asset-id")]
    public string? AssetId { get; set; }

    [HtmlAttributeName("alt")]
    public string Alt { get; set; } = "";

    [HtmlAttributeName("img-width")]
    public int? ImgWidth { get; set; }

    [HtmlAttributeName("img-height")]
    public int? ImgHeight { get; set; }

    [HtmlAttributeName("breakpoints")]
    public string? Breakpoints { get; set; }

    [HtmlAttributeName("sizes")]
    public string Sizes { get; set; } = "(max-width: 768px) 100vw, (max-width: 1200px) 960px, 1280px";

    /// <summary>
    /// When true, uses loading="eager" and fetchpriority="high" (above-fold hero images).
    /// When false, uses loading="lazy" and decoding="async" (below-fold card images).
    /// </summary>
    [HtmlAttributeName("priority")]
    public bool Priority { get; set; }

    [HtmlAttributeName("responsive")]
    public bool Responsive { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (!Responsive || string.IsNullOrEmpty(Src))
            return;

        // Resolve the asset ID from the src path if not explicitly provided
        var resolvedAssetId = AssetId;
        if (string.IsNullOrEmpty(resolvedAssetId) && Src.StartsWith("/blog/assets/"))
        {
            var fileName = Src["/blog/assets/".Length..];
            resolvedAssetId = Path.GetFileNameWithoutExtension(fileName);
        }

        if (string.IsNullOrEmpty(resolvedAssetId))
            return; // Cannot generate responsive variants without an asset ID

        // Parse breakpoints
        var breakpoints = ParseBreakpoints();

        var candidates = breakpoints.Where(w => !ImgWidth.HasValue || w < ImgWidth.Value).ToList();
        if (ImgWidth is > 0) candidates.Add(ImgWidth.Value);
        output.Attributes.SetAttribute("src", Src);
        output.Attributes.SetAttribute("alt", Alt);
        if (candidates.Count > 0)
            output.Attributes.SetAttribute("srcset", string.Join(", ", candidates.Distinct().Select(w => $"{Src}?w={w} {w}w")));
        output.Attributes.SetAttribute("sizes", Sizes);
        if (ImgWidth.HasValue) output.Attributes.SetAttribute("width", ImgWidth.Value);
        if (ImgHeight.HasValue) output.Attributes.SetAttribute("height", ImgHeight.Value);
        output.Attributes.SetAttribute("loading", Priority ? "eager" : "lazy");
        output.Attributes.SetAttribute("decoding", "async");
        if (Priority) output.Attributes.SetAttribute("fetchpriority", "high");
    }

    private int[] ParseBreakpoints()
    {
        if (string.IsNullOrEmpty(Breakpoints))
            return Priority ? DefaultBreakpoints : CardBreakpoints;

        return Breakpoints.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(int.Parse)
            .OrderBy(x => x)
            .ToArray();
    }
}
