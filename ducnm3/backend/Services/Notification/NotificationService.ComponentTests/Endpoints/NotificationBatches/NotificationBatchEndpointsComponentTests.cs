// File: backend/Services/Notification/NotificationService.ComponentTests/Endpoints/NotificationBatches/NotificationBatchEndpointsComponentTests.cs
// Mục đích: Kiểm thử các route Notification Batch qua TestServer, gồm 202 Location, GET detail và failed-items cursor envelope.

#pragma warning disable CA1707

using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging.Abstractions;
using BuildingBlocks.Presentation.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Api.Contracts.NotificationBatches.Requests;
using NotificationService.Api.Endpoints.NotificationBatches.Create;
using NotificationService.Api.Endpoints.NotificationBatches.GetById;
using NotificationService.Api.Endpoints.NotificationBatches.GetDeliveryStatus;
using NotificationService.Api.Endpoints.NotificationBatches.GetFailedItems;
using NotificationService.Api.Endpoints.NotificationBatches.GetList;
using NotificationService.Api.Endpoints.NotificationBatches.GetSnapshotStatus;
using NotificationService.Api.Endpoints.NotificationBatches.RetryFailed;
using NotificationService.Application;
using NotificationService.Application.Repositories;
using NotificationService.Application.Repositories.Models;

namespace NotificationService.ComponentTests.Endpoints.NotificationBatches;

public sealed class NotificationBatchEndpointsComponentTests
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
                3000,
                null));

        Assert.Multiple(() =>
        {
            Assert.That(post.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));
            Assert.That(post.Headers.Location, Is.Not.Null);
            Assert.That(post.Headers.Location!.ToString(), Does.StartWith("/notification/api/notification-batches/"));
        });

        var batchId = post.Headers.Location!
            .ToString()
            .TrimEnd('/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries)[^1];
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

    [Test]
    public async Task GetList_ReturnsOffsetEnvelopeAndDuration()
    {
        await using var fixture = await NotificationBatchApiFixture.CreateAsync();
        fixture.Repository.SetSummary(Guid.NewGuid());

        using var response = await fixture.Client.GetAsync(
            "/api/notification-batches?page=1&pageSize=20&status=PARTIAL_FAILED");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Headers.CacheControl?.NoStore, Is.True);
            Assert.That(body, Does.Contain("\"type\":\"offset\""));
            Assert.That(body, Does.Contain("\"durationMs\":0"));
        });
    }

    [Test]
    public async Task RetryFailed_TerminalBatch_ReturnsAcceptedChildLocation()
    {
        await using var fixture = await NotificationBatchApiFixture.CreateAsync();
        var sourceId = Guid.NewGuid();
        fixture.Repository.SetSummary(sourceId);

        using var response = await fixture.Client.PostAsJsonAsync(
            ApiRoutes.Notifications.BatchRetryFailedServicePath(sourceId),
            new RetryFailedNotificationBatchRequest(Guid.NewGuid().ToString()));
        var body = await response.Content.ReadAsStringAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));
            Assert.That(response.Headers.Location?.ToString(), Does.StartWith(
                "/notification/api/notification-batches/"));
            Assert.That(body, Does.Contain(sourceId.ToString()));
        });
    }

    [Test]
    public async Task GetStepStatuses_ReturnsSnapshotThenDeliveryContracts()
    {
        await using var fixture = await NotificationBatchApiFixture.CreateAsync();
        var batchId = Guid.NewGuid();
        fixture.Repository.SetSummary(batchId);

        using var snapshot = await fixture.Client.GetAsync(
            ApiRoutes.Notifications.BatchSnapshotStatusServicePath(batchId));
        using var delivery = await fixture.Client.GetAsync(
            ApiRoutes.Notifications.BatchDeliveryStatusServicePath(batchId));
        var snapshotBody = await snapshot.Content.ReadAsStringAsync();
        var deliveryBody = await delivery.Content.ReadAsStringAsync();

        Assert.Multiple(() =>
        {
            Assert.That(snapshot.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(snapshot.Headers.CacheControl?.NoStore, Is.True);
            Assert.That(snapshotBody, Does.Contain("\"snapshotCount\":1"));
            Assert.That(delivery.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(deliveryBody, Does.Contain("\"remainingCount\":0"));
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
        app.MapCreateNotificationBatchEndpoint();
        app.MapGetNotificationBatchByIdEndpoint();
        app.MapGetNotificationBatchFailedItemsEndpoint();
        app.MapGetNotificationBatchesEndpoint();
        app.MapRetryFailedNotificationBatchEndpoint();
        app.MapGetNotificationBatchSnapshotStatusEndpoint();
        app.MapGetNotificationBatchDeliveryStatusEndpoint();
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
            record.Title,
            "PENDING",
            0,
            0,
            0,
            0,
            record.BatchSize,
            record.RequestedCount,
            record.SourceBatchId,
            record.CreatedAtUtc,
            null,
            null);
        return Task.FromResult(summary);
    }

    public void SetSummary(Guid batchId) =>
        summary = new NotificationBatchSummary(
            batchId,
            "Batch",
            "PARTIAL_FAILED",
            1,
            1,
            0,
            1,
            500,
            1,
            null,
            DateTime.UnixEpoch,
            DateTime.UnixEpoch,
            DateTime.UnixEpoch);

    public Task<NotificationBatchSummary?> GetByIdAsync(Guid batchId, CancellationToken cancellationToken) =>
        Task.FromResult(summary?.Id == batchId ? summary : null);

    public Task<NotificationBatchSnapshotProgress?> GetSnapshotProgressAsync(
        Guid batchId,
        CancellationToken cancellationToken) =>
        Task.FromResult(summary?.Id == batchId
            ? new NotificationBatchSnapshotProgress(
                batchId, summary.Status, summary.RequestedCount,
                summary.TotalCount, summary.TotalCount)
            : null);

    public Task<NotificationBatchRetryCreation> PrepareRetryAsync(
        Guid sourceBatchId,
        Guid createdBy,
        DateTime createdAtUtc,
        CancellationToken cancellationToken)
    {
        var source = summary!;
        summary = source with
        {
            Id = Guid.NewGuid(),
            Status = "PENDING",
            TotalCount = 0,
            ProcessedCount = 0,
            SuccessCount = 0,
            FailedCount = 0,
            SourceBatchId = sourceBatchId,
            CreatedAtUtc = createdAtUtc,
            StartedAtUtc = null,
            CompletedAtUtc = null,
        };
        return Task.FromResult(new NotificationBatchRetryCreation(summary, true));
    }

    public Task<NotificationBatchSummary> CommitRetryAsync(
        Guid sourceBatchId,
        CancellationToken cancellationToken) => Task.FromResult(summary!);

    public Task<NotificationBatchListPage> ListAsync(
        string? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken) =>
        Task.FromResult(new NotificationBatchListPage(
            summary is null ? [] : [summary], page, pageSize, summary is null ? 0 : 1));

    public Task<NotificationBatchFailedItemsPage> GetFailedItemsAsync(
        Guid batchId,
        Guid? afterItemId,
        int limit,
        CancellationToken cancellationToken) =>
        Task.FromResult(new NotificationBatchFailedItemsPage([], null, false));

    public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task<NotificationSnapshotWork> PrepareSnapshotAsync(Guid batchId, CancellationToken cancellationToken) =>
        Task.FromResult(new NotificationSnapshotWork(false, false));

    public Task<int> AppendSnapshotPageAsync(Guid batchId, IReadOnlyList<Guid> studentIds, CancellationToken cancellationToken) =>
        Task.FromResult(studentIds.Count);

    public Task CopyFailedRecipientsAsync(Guid batchId, Guid sourceBatchId, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public Task<bool> CompleteSnapshotAsync(Guid batchId, CancellationToken cancellationToken) =>
        Task.FromResult(false);

    public Task MarkSnapshotFailedAsync(Guid batchId, CancellationToken cancellationToken) =>
        Task.CompletedTask;

}

internal sealed class StubCommandSender : ICommandSender
{
    public Task SendAsync<TCommand>(
        string destinationService,
        TCommand command,
        CancellationToken cancellationToken = default)
        where TCommand : class, ICommand =>
        Task.CompletedTask;
}
