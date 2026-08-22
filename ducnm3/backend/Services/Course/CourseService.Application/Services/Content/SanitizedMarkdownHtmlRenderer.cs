using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Ganss.Xss;
using Markdig;

namespace CourseService.Application.Services.Content;

public sealed class SanitizedMarkdownHtmlRenderer : IMarkdownHtmlRenderer
{
    private static readonly Regex BlockDollarMathPattern = new(
        @"\$\$(?<latex>[\s\S]+?)\$\$",
        RegexOptions.Compiled);

    private static readonly Regex BlockBracketMathPattern = new(
        @"\\\[(?<latex>[\s\S]+?)\\\]",
        RegexOptions.Compiled);

    private static readonly Regex InlineDollarMathPattern = new(
        @"(?<!\\)\$(?!\$)(?<latex>(?:\\.|[^$\r\n])+?)(?<!\\)\$(?!\$)",
        RegexOptions.Compiled);

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

    public string? Render(string? markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return null;
        }

        var mathExpressions = new List<MathExpression>();
        var markdownWithMathPlaceholders = ReplaceMathDelimiters(markdown, mathExpressions);
        var sanitizedHtml = string.IsNullOrWhiteSpace(gatewayPublicBaseUrl)
            ? sanitizer.Sanitize(Markdown.ToHtml(markdownWithMathPlaceholders, Pipeline))
            : sanitizer.Sanitize(Markdown.ToHtml(markdownWithMathPlaceholders, Pipeline), gatewayPublicBaseUrl);

        return RestoreMathElements(sanitizedHtml, mathExpressions);
    }

    private static string ReplaceMathDelimiters(string markdown, ICollection<MathExpression> expressions)
    {
        var withBracketBlocks = ReplaceMatches(
            markdown,
            BlockBracketMathPattern,
            "block",
            expressions);
        var withDollarBlocks = ReplaceMatches(
            withBracketBlocks,
            BlockDollarMathPattern,
            "block",
            expressions);

        return ReplaceMatches(withDollarBlocks, InlineDollarMathPattern, "inline", expressions);
    }

    private static string ReplaceMatches(
        string markdown,
        Regex pattern,
        string displayMode,
        ICollection<MathExpression> expressions) =>
        pattern.Replace(markdown, match =>
        {
            var latex = match.Groups["latex"].Value;
            if (string.IsNullOrWhiteSpace(latex))
            {
                return match.Value;
            }

            var placeholder = $"COURSE_MATH_{Guid.NewGuid():N}";
            expressions.Add(new MathExpression(placeholder, latex, displayMode));
            return displayMode == "block"
                ? $"\n{placeholder}\n"
                : placeholder;
        });

    private static string RestoreMathElements(string html, IEnumerable<MathExpression> expressions)
    {
        foreach (var expression in expressions)
        {
            var element = $"<span class=\"course-math\" data-math-display=\"{expression.DisplayMode}\" data-math-tex=\"{HtmlEncoder.Default.Encode(expression.Latex)}\"></span>";
            html = html.Replace(expression.Placeholder, element, StringComparison.Ordinal);
        }

        return html;
    }

    private sealed record MathExpression(string Placeholder, string Latex, string DisplayMode);
}
