using BuildingBlocks.DatabaseMigration;
using Microsoft.EntityFrameworkCore;
using StudentService.Application.UseCases.Students.GetList;
using StudentService.Infrastructure.Persistence;
using StudentService.Infrastructure.Persistence.Repositories;
using StudentService.Infrastructure.Persistence.Scaffolded;
using Testcontainers.MySql;

namespace StudentService.IntegrationTests.Persistence;

[NonParallelizable]
public sealed class StudentListRepositoryIntegrationTests
{
    private readonly MySqlContainer mysql = new MySqlBuilder("mysql:8.4")
        .WithDatabase("student_list_tests")
        .WithUsername("student_test")
        .WithPassword("student_test_password")
        .Build();

    [OneTimeSetUp]
    public async Task StartMySqlAsync()
    {
        try
        {
            await mysql.StartAsync();
            await SqlMigrationRunner.ApplyAsync(
                new SqlMigrationRunnerOptions(
                    "student-list-tests",
                    mysql.GetConnectionString(),
                    Path.Combine(
                        TestContext.CurrentContext.TestDirectory,
                        "Database",
                        "Migrations")),
                TestContext.Progress.WriteLine);
        }
        catch
        {
            await mysql.DisposeAsync();
            throw;
        }
    }

    [Test]
    public async Task StatusAndDuplicateSortValuesReturnStablePages()
    {
        var dbOptions = new DbContextOptionsBuilder<StudentDbContext>()
            .UseMySql(
                mysql.GetConnectionString(),
                new MySqlServerVersion(new Version(8, 4, 0)))
            .Options;
        await using var dbContext = new StudentDbContext(dbOptions);
        await dbContext.Students.ExecuteDeleteAsync();
        var sharedCreatedAt =
            new DateTime(2026, 8, 13, 3, 0, 0, DateTimeKind.Utc);
        dbContext.Students.AddRange(
            CreateStudent(
                "11111111-1111-1111-1111-111111111111",
                "first@example.com",
                "First",
                "ACTIVE",
                sharedCreatedAt),
            CreateStudent(
                "22222222-2222-2222-2222-222222222222",
                "second@example.com",
                "Second",
                "ACTIVE",
                sharedCreatedAt),
            CreateStudent(
                "33333333-3333-3333-3333-333333333333",
                "third@example.com",
                "Third",
                "ACTIVE",
                sharedCreatedAt.AddHours(-1)),
            CreateStudent(
                "44444444-4444-4444-4444-444444444444",
                "blocked@example.com",
                "Blocked",
                "BLOCKED",
                sharedCreatedAt.AddHours(1)));
        await dbContext.SaveChangesAsync();
        var repository = new EfStudentRepository(dbContext);

        var firstPage = await repository.GetListAsync(
            GetStudentsQuery.Create("ACTIVE", "createdAt", "desc", 1, 2),
            TestContext.CurrentContext.CancellationToken);
        var secondPage = await repository.GetListAsync(
            GetStudentsQuery.Create("ACTIVE", "createdAt", "desc", 2, 2),
            TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(firstPage.TotalItems, Is.EqualTo(3));
            Assert.That(firstPage.TotalPages, Is.EqualTo(2));
            Assert.That(
                firstPage.Items.All(
                    student => student.CreatedAtUtc.Kind == DateTimeKind.Utc),
                Is.True);
            Assert.That(
                firstPage.Items.Select(student => student.Id),
                Is.EqualTo(
                    new[]
                    {
                        Guid.Parse("22222222-2222-2222-2222-222222222222"),
                        Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    }));
            Assert.That(
                secondPage.Items.Select(student => student.Id),
                Is.EqualTo(
                    new[]
                    {
                        Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    }));
        });
    }

    [OneTimeTearDown]
    public async Task StopMySqlAsync()
    {
        await mysql.DisposeAsync();
    }

    private static Student CreateStudent(
        string id,
        string email,
        string displayName,
        string status,
        DateTime createdAt) =>
        new()
        {
            Id = Guid.Parse(id),
            Email = email,
            DisplayName = displayName,
            Status = status,
            CreatedAt = createdAt,
        };
}
