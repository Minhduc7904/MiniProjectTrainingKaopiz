using CourseService.Application.Services.Content;

namespace CourseService.UnitTests.Application.Content;

public sealed class SanitizedMarkdownHtmlRendererTests
{
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
