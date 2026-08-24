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
    public void ValidateAcceptsConfirmedSeedDatabaseConfiguration()
    {
        var options = CreateValidOptions() with
        {
            StudentConnectionString =
                "Server=localhost;Database=lms_student_seed_db;User ID=test;Password=test;",
            CourseConnectionString =
                "Server=localhost;Database=lms_course_seed_db;User ID=test;Password=test;",
        };

        Assert.DoesNotThrow(() => options.Validate("Development"));
    }

    [Test]
    public void OptimizeCourseSeedSchemaFreshSeedDatabaseRunReturnsTrue()
    {
        var options = CreateValidOptions() with
        {
            CourseConnectionString =
                "Server=localhost;Database=lms_course_seed_db;User ID=test;Password=test;",
        };

        Assert.That(options.OptimizeCourseSeedSchema, Is.True);
    }

    [TestCase(true, false)]
    [TestCase(false, true)]
    public void OptimizeCourseSeedSchemaResumeOrDryRunReturnsFalse(bool resume, bool dryRun)
    {
        var options = CreateValidOptions() with
        {
            CourseConnectionString =
                "Server=localhost;Database=lms_course_seed_db;User ID=test;Password=test;",
            Resume = resume,
            DryRun = dryRun,
        };

        Assert.That(options.OptimizeCourseSeedSchema, Is.False);
    }

    [Test]
    public void OptimizeCourseSeedSchemaDevelopmentDatabaseReturnsFalse()
    {
        var options = CreateValidOptions();

        Assert.That(options.OptimizeCourseSeedSchema, Is.False);
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
    public void ValidateStudentsOnlyWithoutCourseConnectionAcceptsConfiguration()
    {
        var options = CreateValidOptions() with
        {
            CourseConnectionString = string.Empty,
            StudentsOnly = true,
        };

        Assert.DoesNotThrow(() => options.Validate("Development"));
    }

    [Test]
    [NonParallelizable]
    public void ParseOptionsStudentsOnlyDoesNotRequireCourseConnection()
    {
        const string studentConnection =
            "Server=localhost;Database=lms_student_db;User ID=test;Password=test;";
        var originalStudentConnection =
            Environment.GetEnvironmentVariable("SEED_STUDENT_DB_CONNECTION_STRING");
        var originalCourseConnection =
            Environment.GetEnvironmentVariable("SEED_COURSE_DB_CONNECTION_STRING");

        try
        {
            Environment.SetEnvironmentVariable(
                "SEED_STUDENT_DB_CONNECTION_STRING",
                studentConnection);
            Environment.SetEnvironmentVariable("SEED_COURSE_DB_CONNECTION_STRING", null);

            var options = Program.ParseOptions(
                ["--confirm", "--students-only", "--students", "100000"]);

            Assert.Multiple(() =>
            {
                Assert.That(options.StudentsOnly, Is.True);
                Assert.That(options.StudentCount, Is.EqualTo(100_000));
                Assert.That(options.CourseConnectionString, Is.Empty);
            });
        }
        finally
        {
            Environment.SetEnvironmentVariable(
                "SEED_STUDENT_DB_CONNECTION_STRING",
                originalStudentConnection);
            Environment.SetEnvironmentVariable(
                "SEED_COURSE_DB_CONNECTION_STRING",
                originalCourseConnection);
        }
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
    public void ValidateCourseApiLargeProfileAllowsThreeMillionCourses()
    {
        var options = CreateValidOptions() with
        {
            CourseCount = SeedOptions.MaximumCourseApiLargeCourseCount,
            MinLessonsPerCourse = 1,
            MaxLessonsPerCourse = 1,
            CourseApiLarge = true,
        };

        Assert.DoesNotThrow(() => options.Validate("Development"));
    }

    [Test]
    public void ValidateRejectsThreeMillionCoursesWithoutLargeProfile()
    {
        var options = CreateValidOptions() with { CourseCount = 3_000_000 };

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
