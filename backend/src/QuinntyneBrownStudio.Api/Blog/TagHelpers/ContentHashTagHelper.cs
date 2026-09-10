using QuinntyneBrownStudio.Api.Blog.Services;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace QuinntyneBrownStudio.Api.Blog.TagHelpers;

[HtmlTargetElement("link", Attributes = "content-hash")]
[HtmlTargetElement("script", Attributes = "content-hash")]
public class ContentHashTagHelper : TagHelper
{
    private readonly IContentHashService _hashService;

    [HtmlAttributeName("content-hash")]
    public bool ContentHash { get; set; }

    public ContentHashTagHelper(IContentHashService hashService)
    {
        _hashService = hashService;
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        // Remove the content-hash attribute from the output
        output.Attributes.RemoveAll("content-hash");

        // Hash the href attribute (for <link>)
        if (output.Attributes.TryGetAttribute("href", out var hrefAttr) && hrefAttr.Value is string href)
        {
            var hashedHref = _hashService.GetHashedPath(href);
            output.Attributes.SetAttribute("href", hashedHref);
        }

        // Hash the src attribute (for <script>)
        if (output.Attributes.TryGetAttribute("src", out var srcAttr) && srcAttr.Value is string src)
        {
            var hashedSrc = _hashService.GetHashedPath(src);
            output.Attributes.SetAttribute("src", hashedSrc);
        }
    }
}
