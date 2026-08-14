using System.Net;
using System.Net.Http.Json;
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

        var get = await fixture.Client.GetAsync(
            post.Headers.Location!.ToString().Replace("/notification", string.Empty, StringComparison.Ordinal));
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
}

internal sealed class NotificationBatchApiFixture : IAsyncDisposable
{
    private readonly WebApplication app;

    private NotificationBatchApiFixture(WebApplication app, HttpClient client)
    {
        this.app = app;
        Client = client;
    }

    public HttpClient Client { get; }

    public static async Task<NotificationBatchApiFixture> CreateAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddNotificationApplication();
        builder.Services.AddSingleton<IStudentRecipientClient>(
            new StubStudentRecipientClient([Guid.NewGuid()]));
        builder.Services.AddSingleton<INotificationBatchRepository, EndpointBatchRepository>();
        builder.Services.AddSingleton<ICommandSender, StubCommandSender>();

        var app = builder.Build();
        app.UseSharedApiMiddleware();
        app.MapNotificationBatchEndpoints();
        await app.StartAsync();
        return new NotificationBatchApiFixture(app, app.GetTestClient());
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
            checked((uint)record.StudentIds.Count),
            0,
            0,
            0,
            record.BatchSize,
            record.CreatedAtUtc,
            null,
            null);
        return Task.FromResult(summary);
    }

    public Task<NotificationBatchSummary?> GetByIdAsync(Guid batchId, CancellationToken cancellationToken) =>
        Task.FromResult(summary?.Id == batchId ? summary : null);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task<IReadOnlyList<NotificationBatchWorkItem>> ClaimChunkAsync(
        Guid batchId,
        CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<NotificationBatchWorkItem>>([]);

    public Task MarkSuccessAsync(NotificationBatchWorkItem item, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public Task MarkFailureAsync(
        NotificationBatchWorkItem item,
        string errorMessage,
        CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public Task<bool> FinalizeOrHasRemainingAsync(Guid batchId, CancellationToken cancellationToken) =>
        Task.FromResult(false);
}
