using System.Net;
using System.Net.Http.Json;
using System.Text;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Extensions;
using CourseService.Api.Endpoints.Courses.Export;
using CourseService.Application;
using CourseService.Application.Repositories;
using CourseService.Application.UseCases.Courses.Export;
using CourseService.Application.UseCases.Courses.GetList;
using CourseService.Domain.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CA1707

namespace CourseService.ComponentTests.Endpoints;

public sealed class ExportCoursesEndpointComponentTests
{
    private WebApplication app = null!;
    private HttpClient client = null!;
    private StubCourseListRepository repository = null!;

    [SetUp]
    public async Task SetUpAsync()
    {
        repository = new StubCourseListRepository();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton<ICourseListRepository>(repository);
        builder.Services.AddCourseApplication();
        app = builder.Build();
        app.UseSharedApiMiddleware();
        app.MapExportCourses();
        await app.StartAsync();
        client = app.GetTestClient();
    }

    [Test]
    public async Task Export_ValidStatus_StreamsBomEscapedCsvAndNoStoreHeaders()
    {
        using var response = await client.GetAsync(
            string.Concat(ApiRoutes.Courses.ExportServicePath(), "?status=published"),
            TestContext.CurrentContext.CancellationToken);
        var bytes = await response.Content.ReadAsByteArrayAsync(TestContext.CurrentContext.CancellationToken);
        var csv = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("text/csv"));
            Assert.That(response.Content.Headers.ContentDisposition?.FileName, Is.EqualTo("courses.csv"));
            Assert.That(response.Headers.CacheControl?.NoStore, Is.True);
            Assert.That(bytes.Take(3), Is.EqualTo(Encoding.UTF8.GetPreamble()));
            Assert.That(repository.ExportCallCount, Is.EqualTo(1));
            Assert.That(repository.LastQuery?.Status, Is.EqualTo(CourseStatuses.Published));
            Assert.That(csv, Does.Contain("id,name,status,createdAtUtc\r\n"));
            Assert.That(csv, Does.Contain("\"Backend, \"\"Fundamentals\"\"\""));
        });
    }

    [Test]
    public async Task Export_InvalidStatus_ReturnsValidationEnvelopeBeforeOpeningCsvStream()
    {
        using var response = await client.GetAsync(
            string.Concat(ApiRoutes.Courses.ExportServicePath(), "?status=retired"),
            TestContext.CurrentContext.CancellationToken);
        var envelope = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(envelope?.Error.Code, Is.EqualTo(ApiErrorCodes.ValidationFailed));
            Assert.That(repository.ExportCallCount, Is.Zero);
        });
    }

    [TearDown]
    public async Task TearDownAsync()
    {
        client.Dispose();
        await app.DisposeAsync();
    }

    private sealed class StubCourseListRepository : ICourseListRepository
    {
        private readonly CourseExportRow row = new(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "Backend, \"Fundamentals\"",
            CourseStatuses.Published,
            new DateTime(2026, 8, 19, 7, 30, 0, DateTimeKind.Utc));

        public int ExportCallCount { get; private set; }
        public ExportCoursesQuery? LastQuery { get; private set; }

        public Task<IReadOnlyList<CourseListItemRecord>> GetAllAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<CourseListItemRecord>>([]);

        public Task<GetCoursesResult> GetPagedAsync(GetCoursesQuery query, CancellationToken cancellationToken) =>
            Task.FromResult(new GetCoursesResult([], 0, 0));

        public Task<CourseExportChunk> ReadExportChunkAsync(
            ExportCoursesQuery query,
            CourseExportPosition? position,
            CancellationToken cancellationToken)
        {
            ExportCallCount++;
            LastQuery = query;
            return Task.FromResult(position is null
                ? new CourseExportChunk([row])
                : new CourseExportChunk([]));
        }

    }
}
