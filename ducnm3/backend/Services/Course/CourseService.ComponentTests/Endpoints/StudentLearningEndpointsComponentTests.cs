using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Extensions;
using CourseService.Api.Endpoints.Learning;
using CourseService.Application;
using CourseService.Application.Services.Media;
using CourseService.Application.Repositories;
using CourseService.Application.UseCases.Learning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace CourseService.ComponentTests.Endpoints;

public sealed class StudentLearningEndpointsComponentTests
{
    private static readonly Guid StudentId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid CourseId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid LessonId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private WebApplication app = null!;
    private HttpClient client = null!;

    [SetUp]
    public async Task SetUpAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton<IStudentLearningRepository>(new StubStudentLearningRepository());
        builder.Services.AddSingleton<ILessonCommandRepository>(new StubLessonCommandRepository());
        builder.Services.AddSingleton<ICourseMediaReader>(new StubCourseMediaReader());
        builder.Services.AddCourseApplication();
        app = builder.Build();
        app.UseSharedApiMiddleware();
        app.MapGetStudentEnrollments();
        app.MapGetStudentCourseCatalog();
        app.MapGetStudentEnrollmentDetail();
        app.MapGetStudentLessonDetail();
        app.MapGetMyCourseProgress();
        await app.StartAsync();
        client = app.GetTestClient();
    }

    [Test]
    public async Task EnrollmentsReturnStudentOwnedOffsetPageWithThumbnail()
    {
        using var request = StudentRequest(HttpMethod.Get, string.Concat(ApiRoutes.Courses.StudentEnrollmentsServicePath(), "?page=1&pageSize=12"));
        using var response = await client.SendAsync(request, TestContext.CurrentContext.CancellationToken);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.CurrentContext.CancellationToken));

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Headers.CacheControl?.NoStore, Is.True);
            Assert.That(document.RootElement.GetProperty("data")[0].GetProperty("courseId").GetGuid(), Is.EqualTo(CourseId));
            Assert.That(document.RootElement.GetProperty("data")[0].GetProperty("thumbnailUrl").GetString(), Is.EqualTo("https://example.test/thumb.jpg"));
            Assert.That(document.RootElement.GetProperty("meta").GetProperty("pagination").GetProperty("pageSize").GetInt32(), Is.EqualTo(12));
        });
    }

    [Test]
    public async Task ProgressRejectsAdminActor()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, ApiRoutes.Courses.MyProgressServicePath(CourseId));
        request.Headers.Add(ApiHeaderNames.ActorType, ActorHeaderTypes.Admin);
        request.Headers.Add(ApiHeaderNames.ActorId, StudentId.ToString());
        using var response = await client.SendAsync(request, TestContext.CurrentContext.CancellationToken);
        var envelope = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            Assert.That(envelope?.Error.Code, Is.EqualTo(ApiErrorCodes.Forbidden));
        });
    }

    [Test]
    public async Task CatalogReturnsOnlyAvailableCoursesForStudent()
    {
        using var request = StudentRequest(HttpMethod.Get, ApiRoutes.Courses.StudentCourseCatalogServicePath());
        using var response = await client.SendAsync(request, TestContext.CurrentContext.CancellationToken);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.CurrentContext.CancellationToken));

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(document.RootElement.GetProperty("data")[0].GetProperty("courseId").GetGuid(), Is.EqualTo(CourseId));
            Assert.That(document.RootElement.GetProperty("meta").GetProperty("pagination").GetProperty("totalItems").GetInt64(), Is.EqualTo(1));
        });
    }

    [Test]
    public async Task StudentLessonDetailReturnsAttachmentEnvelopeAndNoStore()
    {
        using var request = StudentRequest(HttpMethod.Get, ApiRoutes.Courses.StudentLessonDetailServicePath(CourseId, LessonId));
        using var response = await client.SendAsync(request, TestContext.CurrentContext.CancellationToken);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.CurrentContext.CancellationToken));

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Headers.CacheControl?.NoStore, Is.True);
            Assert.That(document.RootElement.GetProperty("data").GetProperty("id").GetGuid(), Is.EqualTo(LessonId));
            Assert.That(document.RootElement.GetProperty("data").GetProperty("contentHtml").GetString(), Does.Contain("Nội dung"));
            Assert.That(document.RootElement.GetProperty("data").GetProperty("attachments").GetArrayLength(), Is.EqualTo(1));
            Assert.That(document.RootElement.GetProperty("meta").GetProperty("traceId").GetString(), Is.Not.Empty);
        });
    }

    [Test]
    public async Task StudentLessonDetailRejectsMalformedIds()
    {
        using var request = StudentRequest(HttpMethod.Get, $"/api/student/enrollments/not-a-guid/lessons/{LessonId}");
        using var response = await client.SendAsync(request, TestContext.CurrentContext.CancellationToken);
        var envelope = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(envelope?.Error.Code, Is.EqualTo(ApiErrorCodes.ValidationFailed));
        });
    }

    [Test]
    public async Task StudentLessonDetailRejectsAdminActor()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, ApiRoutes.Courses.StudentLessonDetailServicePath(CourseId, LessonId));
        request.Headers.Add(ApiHeaderNames.ActorType, ActorHeaderTypes.Admin);
        request.Headers.Add(ApiHeaderNames.ActorId, StudentId.ToString());
        using var response = await client.SendAsync(request, TestContext.CurrentContext.CancellationToken);
        var envelope = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            Assert.That(envelope?.Error.Code, Is.EqualTo(ApiErrorCodes.Forbidden));
        });
    }

    [Test]
    public async Task StudentLessonDetailReturnsNotFoundEnvelopeWhenLessonDoesNotExist()
    {
        using var request = StudentRequest(HttpMethod.Get, ApiRoutes.Courses.StudentLessonDetailServicePath(CourseId, Guid.NewGuid()));
        using var response = await client.SendAsync(request, TestContext.CurrentContext.CancellationToken);
        var envelope = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(envelope?.Error.Code, Is.EqualTo("LESSON_NOT_FOUND"));
        });
    }

    [TearDown]
    public async Task TearDownAsync()
    {
        client.Dispose();
        await app.DisposeAsync();
    }

    private static HttpRequestMessage StudentRequest(HttpMethod method, string path)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Add(ApiHeaderNames.ActorType, ActorHeaderTypes.Student);
        request.Headers.Add(ApiHeaderNames.ActorId, StudentId.ToString());
        return request;
    }

    private sealed class StubStudentLearningRepository : IStudentLearningRepository
    {
        public Task<StudentEnrollmentsResult> GetEnrollmentsAsync(Guid studentId, GetStudentEnrollmentsQuery query, CancellationToken cancellationToken) =>
            Task.FromResult(new StudentEnrollmentsResult([
                new StudentEnrollmentListItem(Guid.Parse("33333333-3333-3333-3333-333333333333"), CourseId, "Backend Fundamentals", "PUBLISHED", new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 8, 20, 0, 0, 0, DateTimeKind.Utc)),
            ], 1, 1));

        public Task<StudentCourseCatalogResult> GetCatalogAsync(Guid studentId, GetStudentCourseCatalogQuery query, CancellationToken cancellationToken) =>
            Task.FromResult(new StudentCourseCatalogResult([
                new StudentCourseCatalogItem(CourseId, "Backend Fundamentals", "PUBLISHED", new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc)),
            ], 1, 1));

        public Task<bool> IsEnrolledAsync(Guid courseId, Guid studentId, CancellationToken cancellationToken) => Task.FromResult(true);

        public Task<StudentCourseDetailResult?> GetDetailAsync(Guid courseId, Guid studentId, CancellationToken cancellationToken) =>
            Task.FromResult<StudentCourseDetailResult?>(new StudentCourseDetailResult(CourseId, "Backend Fundamentals", "Mô tả", "PUBLISHED", DateTime.UtcNow, []));

        public Task<StudentCourseProgressResult?> GetProgressAsync(Guid courseId, Guid studentId, CancellationToken cancellationToken) =>
            Task.FromResult<StudentCourseProgressResult?>(new StudentCourseProgressResult(CourseId, 2, 1, 50, null));
    }

    private sealed class StubCourseMediaReader : ICourseMediaReader
    {
        private static readonly CourseMediaAsset Thumbnail = new(Guid.NewGuid(), Guid.NewGuid(), CourseId, "https://example.test/original.jpg", "https://example.test/thumb.jpg", null, 0, "IMAGE", "image/jpeg", "thumb.jpg");

        public Task<CourseMediaSet> GetAsync(Guid courseId, CancellationToken cancellationToken) => Task.FromResult(new CourseMediaSet(Thumbnail, []));
        public Task<IReadOnlyDictionary<Guid, CourseMediaSet>> GetManyAsync(IReadOnlyList<Guid> courseIds, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyDictionary<Guid, CourseMediaSet>>(new Dictionary<Guid, CourseMediaSet> { [CourseId] = new(Thumbnail, []) });
        public Task<IReadOnlyList<CourseMediaAsset>> GetLessonAttachmentsAsync(Guid lessonId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<CourseMediaAsset>>([new CourseMediaAsset(Guid.NewGuid(), Guid.NewGuid(), lessonId, "https://example.test/lesson.jpg", null, null, 1, "IMAGE", "image/jpeg", "lesson.jpg")]);
        public Task<IReadOnlyList<Guid>> GetActiveUsageIdsAsync(IReadOnlyList<CourseMediaUsageOwnerScope> owners, CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    private sealed class StubLessonCommandRepository : ILessonCommandRepository
    {
        public Task<LessonCreateRecord?> GetAsync(Guid courseId, Guid lessonId, CancellationToken cancellationToken) =>
            Task.FromResult<LessonCreateRecord?>(lessonId == LessonId ? new LessonCreateRecord(LessonId, CourseId, "Bắt đầu", "# Nội dung", 1, DateTime.UtcNow, DateTime.UtcNow) : null);
        public Task<LessonCreateRecord?> CreateAsync(Guid courseId, string title, string? contentMarkdown, uint? displayOrder, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<LessonCreateRecord?> UpdateAsync(Guid courseId, Guid lessonId, string title, string? contentMarkdown, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<bool> ReorderAsync(Guid courseId, IReadOnlyList<Guid> lessonIds, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<bool> DeleteAsync(Guid courseId, Guid lessonId, CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
