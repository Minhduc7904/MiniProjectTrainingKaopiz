using Ganss.Xss;
using Markdig;

namespace CourseService.Application.Services.Content;

public sealed class SanitizedMarkdownHtmlRenderer : IMarkdownHtmlRenderer
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    private readonly HtmlSanitizer sanitizer = new();
    private readonly string? gatewayPublicBaseUrl;

    public SanitizedMarkdownHtmlRenderer()
    {
        gatewayPublicBaseUrl = Environment
            .GetEnvironmentVariable("Gateway__PublicBaseUrl")
            ?.TrimEnd('/');
        // Markdown media is served through the Gateway. Keep only the attributes
        // required to render that resource; HtmlSanitizer still rejects unsafe URI schemes.
        sanitizer.AllowedTags.Add("img");
        sanitizer.AllowedAttributes.Add("src");
        sanitizer.AllowedAttributes.Add("alt");
        sanitizer.AllowedAttributes.Add("title");
        sanitizer.AllowedSchemes.Clear();
        sanitizer.AllowedSchemes.Add("http");
        sanitizer.AllowedSchemes.Add("https");
        sanitizer.AllowedSchemes.Add("mailto");
    }

    public string? Render(string? markdown) =>
        string.IsNullOrWhiteSpace(markdown)
            ? null
            : string.IsNullOrWhiteSpace(gatewayPublicBaseUrl)
                ? sanitizer.Sanitize(Markdown.ToHtml(markdown, Pipeline))
                : sanitizer.Sanitize(Markdown.ToHtml(markdown, Pipeline), gatewayPublicBaseUrl);
}
