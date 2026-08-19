namespace Lms.DataSeeder.UnitTests;

public class SeedOptionsTests
{
    [Test]
    public void ValidateAcceptsConfirmedDevelopmentConfiguration()
    {
        var options = CreateValidOptions();

        Assert.DoesNotThrow(() => options.Validate("Development"));
    }

    [Test]
    public void ValidateRejectsEnvironmentOtherThanDevelopment()
    {
        var options = CreateValidOptions();

        var exception = Assert.Throws<SeedValidationException>(
            () => options.Validate("Production"));

        Assert.That(exception!.Message, Does.Contain("Development"));
    }

    [Test]
    public void ValidateRejectsWriteWithoutConfirmation()
    {
        var options = CreateValidOptions() with { Confirmed = false };

        var exception = Assert.Throws<SeedValidationException>(
            () => options.Validate("Development"));

        Assert.That(exception!.Message, Does.Contain("--confirm"));
    }

    [Test]
    public void ValidateAllowsDryRunWithoutConfirmation()
    {
        var options = CreateValidOptions() with
        {
            Confirmed = false,
            DryRun = true,
        };

        Assert.DoesNotThrow(() => options.Validate("Development"));
    }

    [Test]
    public void ValidateRejectsUnexpectedDatabaseName()
    {
        var options = CreateValidOptions() with
        {
            CourseConnectionString =
                "Server=localhost;Database=production_course_db;User ID=test;Password=test;",
        };

        var exception = Assert.Throws<SeedValidationException>(
            () => options.Validate("Development"));

        Assert.That(exception!.Message, Does.Contain("lms_course_db"));
    }

    [Test]
    public void ValidateCourseCountAtBenchmarkMaximumAcceptsConfiguration()
    {
        var options = CreateValidOptions() with { CourseCount = 300_000 };

        Assert.DoesNotThrow(() => options.Validate("Development"));
    }

    [Test]
    public void ValidateCourseCountAboveBenchmarkMaximumRejectsConfiguration()
    {
        var options = CreateValidOptions() with { CourseCount = 300_001 };

        Assert.Throws<SeedValidationException>(() => options.Validate("Development"));
    }

    [Test]
    public void DefaultCourseCountRemainsOneHundredThousand()
    {
        Assert.That(SeedOptions.DefaultCourseCount, Is.EqualTo(100_000));
    }

    [TestCase(0, 10)]
    [TestCase(11, 10)]
    public void ValidateRejectsInvalidCourseAssignmentRange(
        int minimum,
        int maximum)
    {
        var options = CreateValidOptions() with
        {
            MinCoursesPerStudent = minimum,
            MaxCoursesPerStudent = maximum,
        };

        Assert.Throws<SeedValidationException>(
            () => options.Validate("Development"));
    }

    private static SeedOptions CreateValidOptions() =>
        new(
            "Server=localhost;Database=lms_student_db;User ID=test;Password=test;",
            "Server=localhost;Database=lms_course_db;User ID=test;Password=test;",
            10,
            10,
            1,
            5,
            1,
            10,
            5,
            123,
            true,
            false,
            false);
}
