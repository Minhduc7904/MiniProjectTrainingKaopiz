using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Extensions;
using CourseService.Api.Endpoints.Courses.GetList;
using CourseService.Application;
using CourseService.Application.Repositories;
using CourseService.Application.UseCases.Courses.GetList;
using CourseService.Application.UseCases.Courses.Export;
using CourseService.Domain.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace CourseService.ComponentTests.Endpoints;

public sealed class GetCoursesEndpointComponentTests
{
    private WebApplication app = null!;
    private HttpClient client = null!;
    private StubCourseListRepository repository = null!;

    [SetUp]
    public async Task SetUpAsync()
    {
        repository = new StubCourseListRepository(new GetCoursesResult(
            [new CourseListItemRecord(
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                "Backend Fundamentals",
                CourseStatuses.Published,
                new DateTime(2026, 8, 19, 1, 0, 0, DateTimeKind.Utc))],
            1,
            1));
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton<ICourseListRepository>(repository);
        builder.Services.AddCourseApplication();
        app = builder.Build();
        app.UseSharedApiMiddleware();
        app.MapGetCourses();
        await app.StartAsync();
        client = app.GetTestClient();
    }

    [Test]
    public async Task DefaultQueryReturnsOffsetPaginationEnvelope()
    {
        using var response = await client.GetAsync(ApiRoutes.Courses.ListServicePath(), TestContext.CurrentContext.CancellationToken);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.CurrentContext.CancellationToken));
        var pagination = document.RootElement.GetProperty("meta").GetProperty("pagination");

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Headers.CacheControl?.NoStore, Is.True);
            Assert.That(document.RootElement.GetProperty("data").GetArrayLength(), Is.EqualTo(1));
            Assert.That(pagination.GetProperty("type").GetString(), Is.EqualTo("offset"));
            Assert.That(pagination.GetProperty("page").GetInt32(), Is.EqualTo(1));
            Assert.That(pagination.GetProperty("pageSize").GetInt32(), Is.EqualTo(20));
            Assert.That(pagination.GetProperty("totalItems").GetInt64(), Is.EqualTo(1));
            Assert.That(pagination.GetProperty("totalPages").GetInt32(), Is.EqualTo(1));
        });
    }

    [Test]
    public async Task InvalidQueryReturnsValidationEnvelopeWithoutCallingRepository()
    {
        using var response = await client.GetAsync(string.Concat(ApiRoutes.Courses.ListServicePath(), "?status=deleted&sortBy=id&sortDirection=sideways&page=0&pageSize=101"), TestContext.CurrentContext.CancellationToken);
        var envelope = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(envelope?.Error.Code, Is.EqualTo(ApiErrorCodes.ValidationFailed));
            Assert.That(envelope?.Error.Details, Has.Count.EqualTo(5));
            Assert.That(repository.PagedCallCount, Is.Zero);
        });
    }

    [TearDown]
    public async Task TearDownAsync()
    {
        client.Dispose();
        await app.DisposeAsync();
    }

    private sealed class StubCourseListRepository(GetCoursesResult result) : ICourseListRepository
    {
        public int PagedCallCount { get; private set; }

        public Task<IReadOnlyList<CourseListItemRecord>> GetAllAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<CourseListItemRecord>>([]);

        public Task<GetCoursesResult> GetPagedAsync(GetCoursesQuery query, CancellationToken cancellationToken)
        {
            PagedCallCount++;
            return Task.FromResult(result);
        }

        public Task<CourseExportChunk> ReadExportChunkAsync(ExportCoursesQuery query, CourseExportPosition? position, CancellationToken cancellationToken) =>
            Task.FromResult(new CourseExportChunk([]));

    }
}
