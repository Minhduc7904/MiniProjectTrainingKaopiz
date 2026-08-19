// File: backend/Services/Media/MediaService.ComponentTests/Endpoints/MediaUsages/GetMediaUsageUrlEndpointsComponentTests.cs
// Mục đích: Kiểm thử endpoint lấy URL Media Usage qua TestServer: route, envelope, phân quyền và response URL.

using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Extensions;
using MediaService.Api.Contracts.Responses;
using MediaService.Api.Endpoints.Media;
using MediaService.Application;
using MediaService.Application.Common.Errors;
using MediaService.Application.Repositories;
using MediaService.Application.Services.Urls;
using MediaService.Application.UseCases.Media.Upload;
using MediaService.Application.UseCases.MediaUsages.GetUrl;
using MediaService.Application.UseCases.MediaUsages.GetUrls;
using MediaService.Domain.Constants;
using MediaService.Domain.Entities;
using MediaService.Domain.ValueObjects;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace MediaService.ComponentTests.Endpoints;

public sealed class GetMediaUsageUrlEndpointsComponentTests
{
    private static readonly Guid UsageId =
        Guid.Parse("62e4f6d2-2af0-4e57-a0d5-8b6ed0edc9f6");
    private static readonly Guid OwnerId =
        Guid.Parse("9c2f278b-01de-4f02-aaf5-099db6a51491");
    private UrlQueryRepository repository = null!;
    private WebApplication app = null!;
    private HttpClient client = null!;

    [SetUp]
    public async Task SetUpAsync()
    {
        repository = new UrlQueryRepository();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddMediaApplication(builder.Configuration);
        builder.Services.AddSingleton<IMediaRepository>(repository);
        builder.Services.AddSingleton<IMediaUsageRepository>(repository);
        builder.Services.AddSingleton<IMediaUrlProvider, FixedMediaUrlProvider>();

        app = builder.Build();
        app.UseSharedApiMiddleware();
        app.MapGetMediaUsageUrl();
        app.MapGetMediaUsageUrls();
        await app.StartAsync();
        client = app.GetTestClient();
    }

    [Test]
    public async Task GetUsageExistsReturnsOkEnvelope()
    {
        // Arrange
        repository.Single = CreateRecord(UsageId, 0);

        // Act
        using var response = await client.GetAsync(
            ApiRoutes.Media.UsageUrlServicePath(UsageId),
            TestContext.CurrentContext.CancellationToken);
        var envelope = await response.Content.ReadFromJsonAsync<
            ApiResponse<MediaUsageUrlResponse>>(
            TestContext.CurrentContext.CancellationToken);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
            Assert.That(response.Headers.CacheControl?.NoStore, Is.True);
            Assert.That(envelope?.Data.UsageId, Is.EqualTo(UsageId));
            Assert.That(envelope?.Data.Url, Is.EqualTo("/media/api/media/fixed/content"));
        });
    }

    [Test]
    public async Task GetOwnerUsagesReturnsEveryUrlInEnvelope()
    {
        // Arrange
        repository.Many =
        [
            CreateRecord(UsageId, 0),
            CreateRecord(Guid.Parse("8d5445e5-43d6-4616-9f1f-5b3499e403c5"), 1),
        ];

        // Act
        using var response = await client.GetAsync(
            $"{ApiRoutes.Media.UsageUrlsServicePath()}?ownerService=STUDENT&ownerType=STUDENT_AVATAR&usageType=AVATAR&ownerId={OwnerId:D}",
            TestContext.CurrentContext.CancellationToken);
        var envelope = await response.Content.ReadFromJsonAsync<
            ApiResponse<MediaUsageUrlResponse[]>>(
            TestContext.CurrentContext.CancellationToken);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(envelope?.Data, Has.Length.EqualTo(2));
            Assert.That(envelope?.Data.Select(item => item.DisplayOrder), Is.EqualTo(new uint[] { 0, 1 }));
        });
    }

    [Test]
    public async Task GetUsageMissingReturnsNotFoundEnvelope()
    {
        // Act
        using var response = await client.GetAsync(
            ApiRoutes.Media.UsageUrlServicePath(UsageId),
            TestContext.CurrentContext.CancellationToken);
        var envelope = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(
            TestContext.CurrentContext.CancellationToken);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(envelope?.Error.Code, Is.EqualTo(MediaErrorCodes.MediaUsageNotFound));
        });
    }

    [TearDown]
    public async Task TearDownAsync()
    {
        client.Dispose();
        await app.DisposeAsync();
    }

    private static MediaUsageUrlRecord CreateRecord(Guid usageId, uint displayOrder) =>
        new(
            new MediaUsageRecord(
                usageId,
                Guid.Parse("7ba4e50e-927a-41a7-aac9-11947646a84f"),
                "STUDENT",
                "STUDENT_AVATAR",
                OwnerId,
                "AVATAR",
                displayOrder,
                DateTime.UnixEpoch),
            new MediaRecord(
                Guid.Parse("7ba4e50e-927a-41a7-aac9-11947646a84f"),
                new("images", "2026/08/14/avatar.png"),
                MediaTypes.Image,
                "image/png",
                "avatar.png",
                10,
                MediaObjectStatuses.Ready,
                DateTime.UnixEpoch,
                null));

    private sealed class FixedMediaUrlProvider : IMediaUrlProvider
    {
        public Task<MediaUrl> GenerateAsync(
            MediaRecord media,
            CancellationToken cancellationToken) =>
            Task.FromResult(new MediaUrl("/media/api/media/fixed/content", null));

        public Task<IReadOnlyList<MediaUrl>> GenerateManyAsync(
            IReadOnlyList<MediaRecord> media,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<MediaUrl>>(
                media.Select(_ => new MediaUrl("/media/api/media/fixed/content", null)).ToArray());
    }

    private sealed class UrlQueryRepository : IMediaRepository, IMediaUsageRepository
    {
        public MediaUsageUrlRecord? Single { get; set; }

        public IReadOnlyList<MediaUsageUrlRecord> Many { get; set; } = [];

        public Task EnsureMediaUsagesAsync(
            IReadOnlyList<CreateMediaUsageRecord> usages,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<MediaUsageUrlRecord?> GetActiveUsageUrlByIdAsync(
            Guid usageId,
            CancellationToken cancellationToken) =>
            Task.FromResult(Single?.Usage.Id == usageId ? Single : null);

        public Task<IReadOnlyList<MediaUsageUrlRecord>> GetActiveUsageUrlsAsync(
            MediaUsageOwnerQuery query,
            CancellationToken cancellationToken) =>
            Task.FromResult(Many);

        public Task AddPendingAsync(
            PendingMediaRecord media,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<MediaRecord?> GetByIdAsync(
            Guid mediaId,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task MarkFailedAsync(
            Guid mediaId,
            string failureReason,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task MarkReadyAsync(
            Guid mediaId,
            string checksumSha256,
            DateTime completedAtUtc,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<MediaUsage> ReplaceMediaThumbnailAsync(
            CreateMediaUsageRecord usage,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<MediaUsage> ReplaceStudentAvatarAsync(
            CreateMediaUsageRecord usage,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }
}
