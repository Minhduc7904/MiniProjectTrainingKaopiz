using CourseService.Application.Services.Content;

namespace CourseService.UnitTests.Application.Content;

public sealed class SanitizedMarkdownHtmlRendererTests
{
    [TestCase("Tính $f'(x)$.", "inline")]
    [TestCase("$$\\boxed{f'(x)}$$", "block")]
    [TestCase("\\[\\boxed{f'(x)}\\]", "block")]
    public void RenderSupportedMathDelimiterEmitsSafeMathElement(
        string markdown,
        string displayMode)
    {
        // Arrange
        var sut = new SanitizedMarkdownHtmlRenderer();

        // Act
        var html = sut.Render(markdown);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(html, Does.Contain("class=\"course-math\""));
            Assert.That(html, Does.Contain($"data-math-display=\"{displayMode}\""));
            Assert.That(html, Does.Not.Contain("<sup>"));
        });
    }

    [Test]
    public void RenderMathContainsHtmlLikeTextEncodesItInsideMathData()
    {
        // Arrange
        var sut = new SanitizedMarkdownHtmlRenderer();

        // Act
        var html = sut.Render("$\\text{<script>alert(1)</script>}$");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(html, Does.Contain("class=\"course-math\""));
            Assert.That(html, Does.Not.Contain("<script>"));
            Assert.That(html, Does.Contain("&lt;script&gt;"));
        });
    }

    [Test]
    public void Render_RelativeMediaContentPath_PrefixesConfiguredGatewayPublicBaseUrl()
    {
        const string gatewayPublicBaseUrl = "https://lms.example.com";
        const string relativeMediaPath = "/media/api/media/40a2f85e-66b2-4279-9014-0fdafab03ff7/content";
        var previousBaseUrl = Environment.GetEnvironmentVariable("Gateway__PublicBaseUrl");

        try
        {
            Environment.SetEnvironmentVariable("Gateway__PublicBaseUrl", gatewayPublicBaseUrl);
            var sut = new SanitizedMarkdownHtmlRenderer();

            var html = sut.Render($"![Screenshot]({relativeMediaPath})");

            Assert.That(
                html,
                Does.Contain($"src=\"{gatewayPublicBaseUrl}{relativeMediaPath}\""));
        }
        finally
        {
            Environment.SetEnvironmentVariable("Gateway__PublicBaseUrl", previousBaseUrl);
        }
    }
}
