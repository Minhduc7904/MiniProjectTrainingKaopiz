using BuildingBlocks.Contracts.Api;

namespace StudentService.UnitTests;

public sealed class ApiRoutesTests
{
    private static readonly Guid ResourceId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Test]
    public void StudentPathsUseServiceAndGatewayShapes()
    {
        Assert.Multiple(() =>
        {
            Assert.That(
                ApiRoutes.Students.GetByIdServicePath(ResourceId),
                Is.EqualTo(
                    "api/students/11111111-1111-1111-1111-111111111111"));
            Assert.That(
                ApiRoutes.Students.GetByIdPublicPath(ResourceId),
                Is.EqualTo(
                    "/student/api/students/11111111-1111-1111-1111-111111111111"));
        });
    }

    [Test]
    public void MediaPathsUseServiceAndGatewayShapes()
    {
        Assert.Multiple(() =>
        {
            Assert.That(ApiRoutes.Media.Upload, Is.EqualTo("/api/media"));
            Assert.That(ApiRoutes.Media.Usages, Is.EqualTo("/api/media/usages"));
            Assert.That(
                ApiRoutes.Media.ContentServicePath(ResourceId),
                Is.EqualTo(
                    "api/media/11111111-1111-1111-1111-111111111111/content"));
            Assert.That(
                ApiRoutes.Media.ContentPublicPath(ResourceId),
                Is.EqualTo(
                    "/media/api/media/11111111-1111-1111-1111-111111111111/content"));
        });
    }
}
