using Microsoft.AspNetCore.Razor.TagHelpers;

namespace QuinntyneBrownStudio.Api.Blog.TagHelpers;

[HtmlTargetElement("resource-hint", TagStructure = TagStructure.WithoutEndTag)]
public class ResourceHintTagHelper : TagHelper
{
    [HtmlAttributeName("type")]
    public string HintType { get; set; } = "preconnect";

    [HtmlAttributeName("href")]
    public string Href { get; set; } = "";

    [HtmlAttributeName("crossorigin")]
    public bool Crossorigin { get; set; }

    [HtmlAttributeName("as")]
    public string? As { get; set; }

    [HtmlAttributeName("nonce")]
    public string? Nonce { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = null;

        if (string.IsNullOrEmpty(Href))
            return;

        var crossoriginAttr = Crossorigin ? " crossorigin" : "";
        var asAttr = !string.IsNullOrEmpty(As) ? $" as=\"{As}\"" : "";
        var nonceAttr = !string.IsNullOrEmpty(Nonce) ? $" nonce=\"{Nonce}\"" : "";

        output.Content.SetHtmlContent(
            $"<link rel=\"{HintType}\" href=\"{Href}\"{asAttr}{crossoriginAttr}{nonceAttr} />");
    }
}
