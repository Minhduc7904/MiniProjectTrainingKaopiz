using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging.Abstractions;
using BuildingBlocks.Presentation.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Api.Contracts.Requests;
using NotificationService.Api.Endpoints;
using NotificationService.Application;
using NotificationService.Application.Abstractions;

namespace NotificationService.UnitTests;

public sealed class NotificationBatchEndpointTests
{
    [Test]
    public async Task PostThenGet_ReturnsAcceptedLocationAndBatchSummary()
    {
        await using var fixture = await NotificationBatchApiFixture.CreateAsync();
        var createdBy = Guid.NewGuid();

        var post = await fixture.Client.PostAsJsonAsync(
            "/api/notification-batches",
            new CreateNotificationBatchRequest(
                "Title",
                "Body",
                "ALL_STUDENTS",
                createdBy.ToString(),
                500,
                null));

        Assert.Multiple(() =>
        {
            Assert.That(post.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));
            Assert.That(post.Headers.Location, Is.Not.Null);
            Assert.That(post.Headers.Location!.ToString(), Does.StartWith("/notification/api/notification-batches/"));
        });

        var batchId = post.Headers.Location!.Segments[^1];
        var get = await fixture.Client.GetAsync(
            string.Concat("/api/notification-batches/", batchId));
        var getBody = await get.Content.ReadAsStringAsync();

        Assert.Multiple(() =>
        {
            Assert.That(get.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(getBody, Does.Contain("\"status\":\"PENDING\""));
            Assert.That(get.Headers.CacheControl?.NoStore, Is.True);
        });
    }

    [Test]
    public async Task Post_UnknownScope_ReturnsValidationFailure()
    {
        await using var fixture = await NotificationBatchApiFixture.CreateAsync();

        var response = await fixture.Client.PostAsJsonAsync(
            "/api/notification-batches",
            new CreateNotificationBatchRequest(
                "Title",
                "Body",
                "STUDENT_IDS",
                Guid.NewGuid().ToString(),
                null,
                null));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetFailedItems_BatchExists_ReturnsCursorEnvelope()
    {
        await using var fixture = await NotificationBatchApiFixture.CreateAsync();
        var batchId = Guid.NewGuid();
        var repository = fixture.Repository;
        repository.SetSummary(batchId);

        using var response = await fixture.Client.GetAsync(
            ApiRoutes.Notifications.BatchFailedItemsServicePath(batchId));
        var body = await response.Content.ReadAsStringAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Headers.CacheControl?.NoStore, Is.True);
            Assert.That(body, Does.Contain("\"items\":[]"));
            Assert.That(body, Does.Contain("\"type\":\"cursor\""));
        });
    }
}

internal sealed class NotificationBatchApiFixture : IAsyncDisposable
{
    private readonly WebApplication app;

    private NotificationBatchApiFixture(
        WebApplication app,
        HttpClient client,
        EndpointBatchRepository repository)
    {
        this.app = app;
        Client = client;
        Repository = repository;
    }

    public HttpClient Client { get; }

    internal EndpointBatchRepository Repository { get; }

    public static async Task<NotificationBatchApiFixture> CreateAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddNotificationApplication();
        var repository = new EndpointBatchRepository();
        builder.Services.AddSingleton<INotificationBatchRepository>(repository);
        builder.Services.AddSingleton<ICommandSender, StubCommandSender>();

        var app = builder.Build();
        app.UseSharedApiMiddleware();
        app.MapNotificationBatchEndpoints();
        await app.StartAsync();
        return new NotificationBatchApiFixture(app, app.GetTestClient(), repository);
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await app.StopAsync();
        await app.DisposeAsync();
    }
}

internal sealed class EndpointBatchRepository : INotificationBatchRepository
{
    private NotificationBatchSummary? summary;

    public Task<NotificationBatchSummary> CreateAsync(
        CreateNotificationBatchRecord record,
        CancellationToken cancellationToken)
    {
        summary = new NotificationBatchSummary(
            record.Id,
            "PENDING",
            0,
            0,
            0,
            0,
            record.BatchSize,
            record.CreatedAtUtc,
            null,
            null);
        return Task.FromResult(summary);
    }

    public void SetSummary(Guid batchId) =>
        summary = new NotificationBatchSummary(
            batchId,
            "PARTIAL_FAILED",
            1,
            1,
            0,
            1,
            500,
            DateTime.UnixEpoch,
            DateTime.UnixEpoch,
            DateTime.UnixEpoch);

    public Task<NotificationBatchSummary?> GetByIdAsync(Guid batchId, CancellationToken cancellationToken) =>
        Task.FromResult(summary?.Id == batchId ? summary : null);

    public Task<NotificationBatchFailedItemsPage> GetFailedItemsAsync(
        Guid batchId,
        Guid? afterItemId,
        int limit,
        CancellationToken cancellationToken) =>
        Task.FromResult(new NotificationBatchFailedItemsPage([], null, false));

    public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task<NotificationSnapshotWork> PrepareSnapshotAsync(Guid batchId, CancellationToken cancellationToken) =>
        Task.FromResult(new NotificationSnapshotWork(false, false));

    public Task AppendSnapshotPageAsync(Guid batchId, IReadOnlyList<Guid> studentIds, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public Task<bool> CompleteSnapshotAsync(Guid batchId, CancellationToken cancellationToken) =>
        Task.FromResult(false);

    public Task MarkSnapshotFailedAsync(Guid batchId, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public Task<NotificationBatchClaim?> ClaimChunkAsync(
        Guid batchId,
        CancellationToken cancellationToken) =>
        Task.FromResult<NotificationBatchClaim?>(null);

    public Task<IReadOnlyList<NotificationSummary>> CompleteClaimAsync(
        NotificationBatchClaim claim,
        IReadOnlyList<NotificationBatchDeliveryResult> results,
        CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<NotificationSummary>>([]);

    public Task<bool> FinalizeOrHasRemainingAsync(Guid batchId, CancellationToken cancellationToken) =>
        Task.FromResult(false);
}
