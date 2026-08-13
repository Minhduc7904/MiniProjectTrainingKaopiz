using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StudentService.Api.Endpoints;
using StudentService.Application;
using StudentService.Application.Features.Students.GetList;

namespace StudentService.ComponentTests.Endpoints;

public sealed class GetStudentsEndpointComponentTests
{
    private WebApplication app = null!;
    private HttpClient client = null!;
    private StubStudentListRepository repository = null!;

    [SetUp]
    public async Task SetUpAsync()
    {
        repository = new StubStudentListRepository(
            new GetStudentsResult(
                [
                    new StudentListItem(
                        Guid.Parse("11111111-1111-1111-1111-111111111111"),
                        "first@example.com",
                        "First Student",
                        "ACTIVE",
                        new DateTime(2026, 8, 13, 1, 0, 0, DateTimeKind.Utc)),
                    new StudentListItem(
                        Guid.Parse("22222222-2222-2222-2222-222222222222"),
                        "second@example.com",
                        "Second Student",
                        "ACTIVE",
                        new DateTime(2026, 8, 13, 0, 0, 0, DateTimeKind.Utc)),
                ],
                42,
                3));

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton<IStudentListRepository>(repository);
        builder.Services.AddStudentApplication();

        app = builder.Build();
        app.UseSharedApiMiddleware();
        app.MapGetStudents();
        await app.StartAsync();
        client = app.GetTestClient();
    }

    [Test]
    public async Task DefaultQueryReturnsOffsetPaginationEnvelope()
    {
        using var response = await client.GetAsync(
            ApiRoutes.Students.ListServicePath(),
            TestContext.CurrentContext.CancellationToken);
        var json = await response.Content.ReadAsStringAsync(
            TestContext.CurrentContext.CancellationToken);
        using var document = JsonDocument.Parse(json);
        var pagination = document.RootElement
            .GetProperty("meta")
            .GetProperty("pagination");

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Headers.CacheControl?.NoStore, Is.True);
            Assert.That(
                document.RootElement.GetProperty("data").GetArrayLength(),
                Is.EqualTo(2));
            Assert.That(
                pagination.GetProperty("type").GetString(),
                Is.EqualTo("offset"));
            Assert.That(pagination.GetProperty("page").GetInt32(), Is.EqualTo(1));
            Assert.That(
                pagination.GetProperty("pageSize").GetInt32(),
                Is.EqualTo(20));
            Assert.That(
                pagination.GetProperty("totalItems").GetInt64(),
                Is.EqualTo(42));
            Assert.That(
                pagination.GetProperty("totalPages").GetInt32(),
                Is.EqualTo(3));
        });
    }

    [Test]
    public async Task AllowlistedQueryMapsNormalizedRepositoryQuery()
    {
        using var response = await client.GetAsync(
            string.Concat(
                ApiRoutes.Students.ListServicePath(),
                "?status=inactive&sortBy=email&sortDirection=asc&page=2&pageSize=10"),
            TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(repository.Query, Is.Not.Null);
            Assert.That(repository.Query!.Status, Is.EqualTo("INACTIVE"));
            Assert.That(repository.Query.SortBy, Is.EqualTo(StudentSortField.Email));
            Assert.That(repository.Query.Descending, Is.False);
            Assert.That(repository.Query.Page, Is.EqualTo(2));
            Assert.That(repository.Query.PageSize, Is.EqualTo(10));
        });
    }

    [Test]
    public async Task InvalidQueryReturnsValidationEnvelope()
    {
        using var response = await client.GetAsync(
            string.Concat(
                ApiRoutes.Students.ListServicePath(),
                "?status=deleted&sortBy=id&sortDirection=sideways&page=0&pageSize=101"),
            TestContext.CurrentContext.CancellationToken);
        var envelope = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(
            TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(
                envelope?.Error.Code,
                Is.EqualTo(ApiErrorCodes.ValidationFailed));
            Assert.That(envelope?.Error.Details, Has.Count.EqualTo(5));
            Assert.That(repository.CallCount, Is.Zero);
        });
    }

    [Test]
    public async Task EmptyPageReturnsSuccessWithZeroTotals()
    {
        repository.Result = new GetStudentsResult([], 0, 0);

        using var response = await client.GetAsync(
            string.Concat(
                ApiRoutes.Students.ListServicePath(),
                "?page=3&pageSize=10"),
            TestContext.CurrentContext.CancellationToken);
        var json = await response.Content.ReadAsStringAsync(
            TestContext.CurrentContext.CancellationToken);
        using var document = JsonDocument.Parse(json);
        var pagination = document.RootElement
            .GetProperty("meta")
            .GetProperty("pagination");

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(
                document.RootElement.GetProperty("data").GetArrayLength(),
                Is.Zero);
            Assert.That(pagination.GetProperty("page").GetInt32(), Is.EqualTo(3));
            Assert.That(
                pagination.GetProperty("totalItems").GetInt64(),
                Is.Zero);
            Assert.That(
                pagination.GetProperty("totalPages").GetInt32(),
                Is.Zero);
        });
    }

    [TearDown]
    public async Task TearDownAsync()
    {
        client.Dispose();
        await app.DisposeAsync();
    }

    private sealed class StubStudentListRepository(
        GetStudentsResult result) : IStudentListRepository
    {
        public int CallCount { get; private set; }

        public GetStudentsQuery? Query { get; private set; }

        public GetStudentsResult Result { get; set; } = result;

        public Task<GetStudentsResult> GetListAsync(
            GetStudentsQuery query,
            CancellationToken cancellationToken)
        {
            CallCount++;
            Query = query;
            return Task.FromResult(Result);
        }
    }
}
