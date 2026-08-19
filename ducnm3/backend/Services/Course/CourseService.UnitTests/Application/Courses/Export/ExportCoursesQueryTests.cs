using CourseService.Application.Common.Errors;
using CourseService.Application.UseCases.Courses.Export;
using CourseService.Domain.Constants;

#pragma warning disable CA1707

namespace CourseService.UnitTests.Application.Courses.Export;

public sealed class ExportCoursesQueryTests
{
    [Test]
    public void Create_StatusContainsWhitespaceAndLowercase_NormalizesToPublished()
    {
        var query = ExportCoursesQuery.Create(" published ");

        Assert.That(query.Status, Is.EqualTo(CourseStatuses.Published));
    }

    [Test]
    public void Create_StatusIsUnsupported_ThrowsStandardCourseValidationException()
    {
        var exception = Assert.Throws<CourseApplicationException>(() => ExportCoursesQuery.Create("retired"));

        Assert.That(exception!.Details, Is.EqualTo([new BuildingBlocks.Contracts.Api.ApiErrorDetail("status", "Status must be DRAFT, PUBLISHED, or ARCHIVED.")]));
    }
}
