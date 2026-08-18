// File: backend/Services/Notification/NotificationService.ComponentTests/Endpoints/Notifications/NotificationEndpointsComponentTests.cs
// Mục đích: Kiểm thử route tạo và đọc Notification qua TestServer, gồm 201 Location, no-store và validation UUID.

#pragma warning disable CA1707

using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging.Abstractions;
using BuildingBlocks.Presentation.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Api.Contracts.Notifications.Requests;
using NotificationService.Api.Endpoints.Notifications.Create;
using NotificationService.Api.Endpoints.Notifications.GetById;
using NotificationService.Application;
using NotificationService.Application.Repositories;
using NotificationService.Application.Repositories.Models;
using NotificationService.Domain.Constants;

namespace NotificationService.ComponentTests.Endpoints.Notifications;

public sealed class NotificationEndpointsComponentTests
{
    [Test]
    public async Task PostThenGetReturnsCreatedLocationAndNotification()
    {
        await using var fixture = await NotificationApiFixture.CreateAsync();
        var post = await fixture.Client.PostAsJsonAsync(
            ApiRoutes.Notifications.Items,
            new CreateNotificationRequest(
                Guid.NewGuid().ToString(),
                "Title",
                "Body",
                Guid.NewGuid().ToString()));

        Assert.Multiple(() =>
        {
            Assert.That(post.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(post.Headers.Location, Is.Not.Null);
            Assert.That(post.Headers.Location!.ToString(), Does.StartWith("/notification/api/notifications/"));
        });

        var id = post.Headers.Location!.ToString().Split('/')[^1];
        var get = await fixture.Client.GetAsync($"/api/notifications/{id}");
        var body = await get.Content.ReadAsStringAsync();

        Assert.Multiple(() =>
        {
            Assert.That(get.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(get.Headers.CacheControl?.NoStore, Is.True);
            Assert.That(body, Does.Contain("\"status\":\"UNREAD\""));
        });
    }

    [Test]
    public async Task PostWithInvalidStudentIdReturnsValidationFailure()
    {
        await using var fixture = await NotificationApiFixture.CreateAsync();
        var response = await fixture.Client.PostAsJsonAsync(
            ApiRoutes.Notifications.Items,
            new CreateNotificationRequest("invalid", "Title", "Body", Guid.NewGuid().ToString()));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}

internal sealed class NotificationApiFixture : IAsyncDisposable
{
    private readonly WebApplication app;

    private NotificationApiFixture(WebApplication app, HttpClient client)
    {
        this.app = app;
        Client = client;
    }

    public HttpClient Client { get; }

    public static async Task<NotificationApiFixture> CreateAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddNotificationApplication();
        builder.Services.AddSingleton<INotificationRepository, EndpointNotificationRepository>();
        builder.Services.AddSingleton<ICommandSender, NotificationCommandSender>();

        var app = builder.Build();
        app.UseSharedApiMiddleware();
        app.MapCreateNotificationEndpoint();
        app.MapGetNotificationByIdEndpoint();
        await app.StartAsync();
        return new NotificationApiFixture(app, app.GetTestClient());
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await app.StopAsync();
        await app.DisposeAsync();
    }
}

internal sealed class EndpointNotificationRepository : INotificationRepository
{
    private NotificationSummary? notification;

    public Task<NotificationSummary> CreateAsync(
        CreateNotificationRecord record,
        CancellationToken cancellationToken)
    {
        notification = new NotificationSummary(
            record.Id,
            record.RecipientStudentId,
            record.Title,
            record.BodyMarkdown,
            NotificationSourceTypes.Direct,
            "UNREAD",
            record.CreatedBy,
            record.CreatedAtUtc,
            null);
        return Task.FromResult(notification);
    }

    public Task<NotificationSummary?> GetByIdAsync(Guid notificationId, CancellationToken cancellationToken) =>
        Task.FromResult(notification?.Id == notificationId ? notification : null);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

internal sealed class NotificationCommandSender : ICommandSender
{
    public Task SendAsync<TCommand>(
        string destinationService,
        TCommand command,
        CancellationToken cancellationToken = default)
        where TCommand : class, ICommand =>
        Task.CompletedTask;
}
