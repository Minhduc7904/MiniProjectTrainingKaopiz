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
                ApiRoutes.Students.ListServicePath(),
                Is.EqualTo("api/students"));
            Assert.That(
                ApiRoutes.Students.ListPublicPath(),
                Is.EqualTo("/student/api/students"));
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
    public void StudentAuthPathsUseServiceAndGatewayShapes()
    {
        Assert.Multiple(() =>
        {
            Assert.That(ApiRoutes.StudentAuth.Register, Is.EqualTo("/api/auth/register"));
            Assert.That(ApiRoutes.StudentAuth.Login, Is.EqualTo("/api/auth/login"));
            Assert.That(ApiRoutes.StudentAuth.MeServicePath(), Is.EqualTo("api/auth/me"));
            Assert.That(ApiRoutes.StudentAuth.MePublicPath(), Is.EqualTo("/student/api/auth/me"));
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
