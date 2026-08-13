using System.Globalization;
using BuildingBlocks.DatabaseMigration;
using MySqlConnector;
using Testcontainers.MySql;

namespace Lms.DataSeeder.IntegrationTests;

[NonParallelizable]
public class MySqlSeedRunnerTests
{
    private MySqlContainer studentDatabase = null!;
    private MySqlContainer courseDatabase = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        studentDatabase = new MySqlBuilder("mysql:8.4")
            .WithDatabase("lms_student_db")
            .WithUsername("student_test")
            .WithPassword("student_test_password")
            .Build();
        courseDatabase = new MySqlBuilder("mysql:8.4")
            .WithDatabase("lms_course_db")
            .WithUsername("course_test")
            .WithPassword("course_test_password")
            .Build();

        await Task.WhenAll(
            studentDatabase.StartAsync(),
            courseDatabase.StartAsync());

        await SqlMigrationRunner.ApplyAsync(
            new SqlMigrationRunnerOptions(
                "student-test",
                studentDatabase.GetConnectionString(),
                Path.Combine(AppContext.BaseDirectory, "Migrations", "Student")),
            TestContext.Progress.WriteLine);
        await SqlMigrationRunner.ApplyAsync(
            new SqlMigrationRunnerOptions(
                "course-test",
                courseDatabase.GetConnectionString(),
                Path.Combine(AppContext.BaseDirectory, "Migrations", "Course")),
            TestContext.Progress.WriteLine);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await studentDatabase.DisposeAsync();
        await courseDatabase.DisposeAsync();
    }

    [Test]
    public async Task RunAsyncSeedsRelationshipsAndResumeIsIdempotent()
    {
        var options = CreateOptions(resume: false);
        var runner = new MySqlSeedRunner(options);

        var firstRun = await runner.RunAsync("Development");

        await using var studentConnection =
            new MySqlConnection(studentDatabase.GetConnectionString());
        await using var courseConnection =
            new MySqlConnection(courseDatabase.GetConnectionString());
        await studentConnection.OpenAsync();
        await courseConnection.OpenAsync();

        var studentCount = await CountAsync(studentConnection, "students");
        var courseCount = await CountAsync(courseConnection, "courses");
        var lessonCount = await CountAsync(courseConnection, "lessons");
        var enrollmentCount = await CountAsync(courseConnection, "enrollments");
        var duplicateEnrollmentGroups = await ScalarAsync(
            courseConnection,
            """
            SELECT COUNT(*) FROM (
                SELECT course_id, student_id
                FROM enrollments
                GROUP BY course_id, student_id
                HAVING COUNT(*) > 1
            ) AS duplicates;
            """);

        Assert.Multiple(() =>
        {
            Assert.That(studentCount, Is.EqualTo(30));
            Assert.That(courseCount, Is.EqualTo(20));
            Assert.That(lessonCount, Is.EqualTo(firstRun.Plan.Lessons));
            Assert.That(enrollmentCount, Is.EqualTo(firstRun.Plan.Enrollments));
            Assert.That(duplicateEnrollmentGroups, Is.Zero);
        });

        var resumeOptions = options with { Resume = true };
        var secondRun = await new MySqlSeedRunner(resumeOptions)
            .RunAsync("Development");

        Assert.Multiple(() =>
        {
            Assert.That(secondRun.InsertedRows, Is.Zero);
            Assert.That(secondRun.SkippedRows, Is.EqualTo(firstRun.Plan.TotalRows));
        });

        var unsafeFreshRun = new MySqlSeedRunner(options);
        var exception = Assert.ThrowsAsync<SeedValidationException>(
            async () => await unsafeFreshRun.RunAsync("Development"));
        Assert.That(exception!.Message, Does.Contain("--resume"));
    }

    private SeedOptions CreateOptions(bool resume) =>
        new(
            studentDatabase.GetConnectionString(),
            courseDatabase.GetConnectionString(),
            30,
            20,
            1,
            5,
            1,
            10,
            25,
            12345,
            true,
            resume,
            false);

    private static Task<long> CountAsync(
        MySqlConnection connection,
        string table) =>
        ScalarAsync(connection, $"SELECT COUNT(*) FROM `{table}`;");

    private static async Task<long> ScalarAsync(
        MySqlConnection connection,
        string sql)
    {
        await using var command = new MySqlCommand(sql, connection);
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt64(result, CultureInfo.InvariantCulture);
    }
}
