// File: backend/Services/Notification/NotificationService.IntegrationTests/Persistence/NotificationBatchSnapshotIntegrationTests.cs
// Mục đích: Chứng minh UPSERT snapshot recipient chạy được trên MySQL production mà không mơ hồ cột id.

using BuildingBlocks.DatabaseMigration;
using Microsoft.EntityFrameworkCore;
using NotificationService.Application.UseCases.NotificationBatches;
using NotificationService.Domain.Constants;
using NotificationService.Infrastructure.Persistence.Context;
using NotificationService.Infrastructure.Persistence.Repositories;
using NotificationService.Infrastructure.Persistence.Scaffolded;
using Testcontainers.MySql;

namespace NotificationService.IntegrationTests.Persistence;

[NonParallelizable]
public sealed class NotificationBatchSnapshotIntegrationTests
{
    private readonly MySqlContainer mysql = new MySqlBuilder("mysql:8.4")
        .WithDatabase("notification_batch_snapshot_tests")
        .WithUsername("notification_test")
        .WithPassword("notification_test_password")
        .Build();

    [OneTimeSetUp]
    public async Task StartMySqlAsync()
    {
        try
        {
            await mysql.StartAsync();
            await SqlMigrationRunner.ApplyAsync(
                new SqlMigrationRunnerOptions(
                    "notification-batch-snapshot-tests",
                    mysql.GetConnectionString(),
                    Path.Combine(TestContext.CurrentContext.TestDirectory, "Database", "Migrations")),
                TestContext.Progress.WriteLine);
        }
        catch
        {
            await mysql.DisposeAsync();
            throw;
        }
    }

    [Test]
    public async Task AppendSnapshotPageAsync_RedeliveredRecipients_DoesNotDuplicateItems()
    {
        var batchId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var firstStudentId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var secondStudentId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var dbOptions = CreateDbOptions();
        await using (var seedContext = new NotificationDbContext(dbOptions))
        {
            seedContext.NotificationBatches.Add(new NotificationBatch
            {
                Id = batchId,
                Title = "Snapshot batch",
                BodyMarkdown = "Body",
                TargetScope = NotificationTargetScopes.AllStudents,
                CreatedBy = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Status = NotificationBatchStatuses.Snapshotting,
                BatchSize = 500,
                CreatedAt = DateTime.UtcNow,
            });
            await seedContext.SaveChangesAsync();
        }

        await using var context = new NotificationDbContext(dbOptions);
        var repository = new EfNotificationBatchRepository(
            context,
            new TestNotificationDbContextFactory(dbOptions),
            TimeProvider.System,
            new NotificationBatchProcessingOptions());

        var inserted = await repository.AppendSnapshotPageAsync(
            batchId,
            [firstStudentId, secondStudentId],
            CancellationToken.None);
        var redeliveredInserted = await repository.AppendSnapshotPageAsync(
            batchId,
            [firstStudentId, secondStudentId],
            CancellationToken.None);

        await using var verifyContext = new NotificationDbContext(dbOptions);
        var items = await verifyContext.NotificationBatchItems.AsNoTracking()
            .Where(item => item.BatchId == batchId)
            .OrderBy(item => item.StudentId)
            .ToListAsync();
        Assert.Multiple(() =>
        {
            Assert.That(inserted, Is.EqualTo(2));
            Assert.That(redeliveredInserted, Is.EqualTo(0));
            Assert.That(items, Has.Count.EqualTo(2));
            Assert.That(items.Select(item => item.StudentId), Is.EquivalentTo([firstStudentId, secondStudentId]));
            Assert.That(items.Select(item => item.Status), Is.All.EqualTo(NotificationBatchItemStatuses.Pending));
        });
    }

    [OneTimeTearDown]
    public async Task StopMySqlAsync()
    {
        await mysql.DisposeAsync();
    }

    private DbContextOptions<NotificationDbContext> CreateDbOptions() =>
        new DbContextOptionsBuilder<NotificationDbContext>()
            .UseMySql(
                mysql.GetConnectionString(),
                new MySqlServerVersion(new Version(8, 4, 0)))
            .Options;

    private sealed class TestNotificationDbContextFactory(
        DbContextOptions<NotificationDbContext> options) : IDbContextFactory<NotificationDbContext>
    {
        public NotificationDbContext CreateDbContext() => new(options);

        public Task<NotificationDbContext> CreateDbContextAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult(CreateDbContext());
    }
}
