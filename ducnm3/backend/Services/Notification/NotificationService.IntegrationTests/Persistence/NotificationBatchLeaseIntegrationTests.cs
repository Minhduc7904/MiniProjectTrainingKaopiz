using BuildingBlocks.DatabaseMigration;
using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Abstractions;
using NotificationService.Application.Features.Batches;
using NotificationService.Domain.Notifications;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Persistence.Scaffolded;
using Testcontainers.MySql;

namespace NotificationService.IntegrationTests.Persistence;

[NonParallelizable]
public sealed class NotificationBatchLeaseIntegrationTests
{
    private readonly MySqlContainer mysql = new MySqlBuilder("mysql:8.4")
        .WithDatabase("notification_batch_lease_tests")
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
                    "notification-batch-lease-tests",
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
    public async Task ClaimChunkAsync_ConcurrentWorkers_ReservesDisjointItemsAndReclaimsExpiredLease()
    {
        var batchId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var expiredItemId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var pendingItemId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var dbOptions = CreateDbOptions();
        await using (var seedContext = new NotificationDbContext(dbOptions))
        {
            seedContext.NotificationBatches.Add(new NotificationBatch
            {
                Id = batchId,
                Title = "Batch",
                BodyMarkdown = "Body",
                TargetScope = NotificationTargetScopes.AllStudents,
                CreatedBy = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Status = NotificationBatchStatuses.SnapshotReady,
                BatchSize = 1,
                CreatedAt = DateTime.UtcNow,
            });
            seedContext.NotificationBatchItems.AddRange(
                new NotificationBatchItem
                {
                    Id = expiredItemId,
                    BatchId = batchId,
                    StudentId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                    Status = NotificationBatchItemStatuses.Processing,
                    LeaseToken = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                    LeaseExpiresAt = DateTime.UtcNow.AddMinutes(-1),
                },
                new NotificationBatchItem
                {
                    Id = pendingItemId,
                    BatchId = batchId,
                    StudentId = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                    Status = NotificationBatchItemStatuses.Pending,
                });
            await seedContext.SaveChangesAsync();
        }

        await using var firstContext = new NotificationDbContext(dbOptions);
        await using var secondContext = new NotificationDbContext(dbOptions);
        var factory = new TestNotificationDbContextFactory(dbOptions);
        var processingOptions = new NotificationBatchProcessingOptions
        {
            ClaimLeaseSeconds = 120,
        };
        var firstRepository = new EfNotificationBatchRepository(
            firstContext,
            factory,
            TimeProvider.System,
            processingOptions);
        var secondRepository = new EfNotificationBatchRepository(
            secondContext,
            factory,
            TimeProvider.System,
            processingOptions);

        var firstClaim = await firstRepository.ClaimChunkAsync(batchId, CancellationToken.None);
        var secondClaim = await secondRepository.ClaimChunkAsync(batchId, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(firstClaim, Is.Not.Null);
            Assert.That(secondClaim, Is.Not.Null);
            Assert.That(firstClaim!.Items, Has.Count.EqualTo(1));
            Assert.That(secondClaim!.Items, Has.Count.EqualTo(1));
            Assert.That(firstClaim.Items[0].Id, Is.Not.EqualTo(secondClaim.Items[0].Id));
            Assert.That(
                new[] { firstClaim.Items[0].Id, secondClaim.Items[0].Id },
                Is.EquivalentTo(new[] { expiredItemId, pendingItemId }));
        });

        await using var verifyContext = new NotificationDbContext(dbOptions);
        var claimedItems = await verifyContext.NotificationBatchItems
            .AsNoTracking()
            .Where(item => item.BatchId == batchId)
            .ToListAsync();
        Assert.That(
            claimedItems.All(item =>
                item.Status == NotificationBatchItemStatuses.Processing &&
                item.LeaseToken is not null &&
                item.LeaseExpiresAt > DateTime.UtcNow),
            Is.True);
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
