using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging.Abstractions;
using BuildingBlocks.Presentation.Extensions;
using CourseService.Api.Endpoints.LessonAi.CreateJob;
using CourseService.Api.Endpoints.LessonAi.GetDocuments;
using CourseService.Api.Endpoints.LessonAi.GetJob;
using CourseService.Api.Endpoints.LessonAi.GetJobs;
using CourseService.Api.Endpoints.LessonAi.SyncDocuments;
using CourseService.Application;
using CourseService.Application.Repositories;
using CourseService.Application.Services.Ai;
using CourseService.Application.Services.Media;
using CourseService.Application.UseCases.LessonAi.Processing;
using CourseService.ComponentTests.Support;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace CourseService.ComponentTests.Endpoints;

public sealed class LessonAiEndpointsComponentTests
{
    private static readonly Guid AdminId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid StudentId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    private static readonly string[] ExpectedInvalidFields = ["jobType", "mediaIds"];

    private WebApplication app = null!;
    private HttpClient client = null!;
    private InMemoryQuizStore quizzes = null!;
    private InMemoryLessonAiStore aiStore = null!;
    private FakeCourseMediaReader attachments = null!;
    private FakeMediaContentReader files = null!;
    private MutableLessonRepository lessons = null!;
    private SwitchableAiCompletionClient model = null!;

    [SetUp]
    public async Task SetUpAsync()
    {
        quizzes = new InMemoryQuizStore();
        aiStore = new InMemoryLessonAiStore();
        attachments = new FakeCourseMediaReader();
        files = new FakeMediaContentReader();
        lessons = new MutableLessonRepository(quizzes.CourseId, quizzes.LessonId);
        model = new SwitchableAiCompletionClient(new ScriptedAiCompletionClient(
            () => ScriptedAiCompletionClient.Tool(SaveLessonContentToolSet.ToolName, JsonSerializer.Serialize(new { markdown = "# Bài học\n\n" + new string('a', 150) }))));

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton<IQuizCommandRepository>(quizzes);
        builder.Services.AddSingleton<ILessonCommandRepository>(lessons);
        builder.Services.AddSingleton<ILessonAiRepository>(aiStore);
        builder.Services.AddSingleton<ICourseMediaReader>(attachments);
        builder.Services.AddSingleton<IMediaContentReader>(files);
        builder.Services.AddSingleton<IAiCompletionClient>(model);
        builder.Services.AddSingleton<ICommandSender>(new RecordingCommandSender());
        builder.Services.AddCourseApplication();
        app = builder.Build();
        app.UseSharedApiMiddleware();
        app.MapGetLessonAiDocuments();
        app.MapSyncLessonAiDocuments();
        app.MapCreateLessonAiJob();
        app.MapGetLessonAiJobs();
        app.MapGetLessonAiJob();
        await app.StartAsync();
        client = app.GetTestClient();
    }

    [TearDown]
    public async Task TearDownAsync()
    {
        client.Dispose();
        await app.DisposeAsync();
    }

    private string Path(string template, Guid? jobId = null) =>
        template
            .Replace("{courseId}", quizzes.CourseId.ToString(), StringComparison.Ordinal)
            .Replace("{lessonId}", quizzes.LessonId.ToString(), StringComparison.Ordinal)
            .Replace("{jobId}", jobId?.ToString() ?? string.Empty, StringComparison.Ordinal);

    private static HttpRequestMessage Request(HttpMethod method, string path, string? actorType = ActorHeaderTypes.Admin, Guid? actorId = null, object? body = null)
    {
        var request = new HttpRequestMessage(method, path);
        if (actorType is not null)
        {
            request.Headers.Add(ApiHeaderNames.ActorType, actorType);
            request.Headers.Add(ApiHeaderNames.ActorId, (actorId ?? AdminId).ToString());
        }

        if (body is not null) request.Content = JsonContent.Create(body);
        return request;
    }

    private async Task<(HttpResponseMessage Response, JsonDocument Json)> SendAsync(HttpRequestMessage request)
    {
        var response = await client.SendAsync(request, TestContext.CurrentContext.CancellationToken);
        var text = await response.Content.ReadAsStringAsync(TestContext.CurrentContext.CancellationToken);
        request.Dispose();
        return (response, JsonDocument.Parse(string.IsNullOrEmpty(text) ? "{}" : text));
    }

    private static string ErrorCode(JsonDocument json) => json.RootElement.GetProperty("error").GetProperty("code").GetString()!;

    private Guid AddTextAttachment(string name = "bai.txt", string text = "Nội dung tài liệu")
    {
        var attachment = attachments.Add(quizzes.LessonId, name, "text/plain");
        files.Files[attachment.MediaId] = System.Text.Encoding.UTF8.GetBytes(text);
        return attachment.MediaId;
    }

    private async Task RunWorkerUntilIdleAsync()
    {
        using var scope = app.Services.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<LessonAiWorkRunner>();
        while (await runner.RunNextAsync(TestContext.CurrentContext.CancellationToken))
        {
        }
    }

    [Test]
    public async Task SyncQueuesAttachmentsAndTheWorkerExtractsThemIntoTheDocumentList()
    {
        var mediaId = AddTextAttachment();
        attachments.Add(quizzes.LessonId, "clip.mp4", "video/mp4", order: 2);

        var (sync, syncJson) = await SendAsync(Request(HttpMethod.Post, Path(ApiRoutes.Courses.LessonAiDocumentsSyncTemplate)));
        await RunWorkerUntilIdleAsync();
        var (list, listJson) = await SendAsync(Request(HttpMethod.Get, Path(ApiRoutes.Courses.LessonAiDocumentsTemplate)));

        var items = listJson.RootElement.GetProperty("data").GetProperty("items");
        Assert.Multiple(() =>
        {
            Assert.That(sync.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(syncJson.RootElement.GetProperty("data").GetProperty("items")[0].GetProperty("status").GetString(), Is.EqualTo("PENDING"));
            Assert.That(list.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(list.Headers.CacheControl?.NoStore, Is.True);
            Assert.That(items[0].GetProperty("mediaId").GetGuid(), Is.EqualTo(mediaId));
            Assert.That(items[0].GetProperty("status").GetString(), Is.EqualTo("EXTRACTED"));
            Assert.That(items[0].GetProperty("textLength").GetInt32(), Is.GreaterThan(0));
            Assert.That(items[1].GetProperty("status").GetString(), Is.EqualTo("UNSUPPORTED"));
            Assert.That(listJson.RootElement.GetProperty("data").GetRawText(), Does.Not.Contain("Nội dung tài liệu"));
        });
    }

    [Test]
    public async Task SyncWithRetryFailedRequeuesFailedDocuments()
    {
        var mediaId = AddTextAttachment();
        files.Files.Remove(mediaId);
        await SendAsync(Request(HttpMethod.Post, Path(ApiRoutes.Courses.LessonAiDocumentsSyncTemplate)));
        await RunWorkerUntilIdleAsync();

        var (_, retryJson) = await SendAsync(Request(HttpMethod.Post, Path(ApiRoutes.Courses.LessonAiDocumentsSyncTemplate) + "?retryFailed=true"));

        Assert.That(retryJson.RootElement.GetProperty("data").GetProperty("items")[0].GetProperty("status").GetString(), Is.EqualTo("PENDING"));
    }

    [Test]
    public async Task CreateLessonJobReturnsAcceptedAndTheWorkerWritesTheLessonContent()
    {
        AddTextAttachment();
        await SendAsync(Request(HttpMethod.Post, Path(ApiRoutes.Courses.LessonAiDocumentsSyncTemplate)));
        await RunWorkerUntilIdleAsync();
        var mediaId = aiStore.Documents.Single().MediaId;

        var (response, json) = await SendAsync(Request(HttpMethod.Post, Path(ApiRoutes.Courses.LessonAiJobsTemplate), body: new { jobType = "SYNTHESIZE_LESSON", mediaIds = new[] { mediaId } }));
        var jobId = json.RootElement.GetProperty("data").GetProperty("id").GetGuid();
        await RunWorkerUntilIdleAsync();
        var (_, detail) = await SendAsync(Request(HttpMethod.Get, Path(ApiRoutes.Courses.LessonAiJobByIdTemplate, jobId)));

        var job = detail.RootElement.GetProperty("data").GetProperty("job");
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));
            Assert.That(response.Headers.Location?.ToString(), Does.EndWith(jobId.ToString()));
            Assert.That(json.RootElement.GetProperty("data").GetProperty("status").GetString(), Is.EqualTo("PENDING"));
            Assert.That(job.GetProperty("status").GetString(), Is.EqualTo("COMPLETED"));
            Assert.That(job.GetProperty("stepCount").GetInt32(), Is.EqualTo(1));
            Assert.That(lessons.Content, Does.StartWith("# Bài học"));
            Assert.That(((ScriptedAiCompletionClient)model.Current).Requests.Single().Task, Is.EqualTo("lesson-synthesis"));
        });
    }

    [Test]
    public async Task CreateQuizJobProducesADraftQuizLinkedFromTheJob()
    {
        AddTextAttachment();
        await SendAsync(Request(HttpMethod.Post, Path(ApiRoutes.Courses.LessonAiDocumentsSyncTemplate)));
        await RunWorkerUntilIdleAsync();
        var mediaId = aiStore.Documents.Single().MediaId;
        var quizArguments = JsonSerializer.Serialize(new
        {
            title = "Quiz AI",
            questions = new[]
            {
                new
                {
                    questionMarkdown = "Câu hỏi?",
                    questionType = "SINGLE_CHOICE",
                    points = 1,
                    options = new[] { new { optionMarkdown = "A", isCorrect = true }, new { optionMarkdown = "B", isCorrect = false } },
                },
            },
        });
        model.Current = new ScriptedAiCompletionClient(() => ScriptedAiCompletionClient.Tool(CreateQuizToolSet.ToolName, quizArguments));

        var (_, created) = await SendAsync(Request(HttpMethod.Post, Path(ApiRoutes.Courses.LessonAiJobsTemplate), body: new { jobType = "GENERATE_QUIZ", mediaIds = new[] { mediaId }, questionCount = 1 }));
        await RunWorkerUntilIdleAsync();
        var (_, list) = await SendAsync(Request(HttpMethod.Get, Path(ApiRoutes.Courses.LessonAiJobsTemplate)));

        var job = list.RootElement.GetProperty("data").GetProperty("items")[0];
        var quiz = quizzes.Quizzes.Values.Single();
        Assert.Multiple(() =>
        {
            Assert.That(created.RootElement.GetProperty("data").GetProperty("questionCount").GetInt32(), Is.EqualTo(1));
            Assert.That(job.GetProperty("status").GetString(), Is.EqualTo("COMPLETED"));
            Assert.That(job.GetProperty("resultQuizId").GetGuid(), Is.EqualTo(quiz.Id));
            Assert.That(quiz.Status, Is.EqualTo("DRAFT"));
        });
    }

    [Test]
    public async Task FailedJobExposesTheErrorCodeWithoutLeakingInternals()
    {
        AddTextAttachment();
        await SendAsync(Request(HttpMethod.Post, Path(ApiRoutes.Courses.LessonAiDocumentsSyncTemplate)));
        await RunWorkerUntilIdleAsync();
        model.Current = new ThrowingAiCompletionClient(new AiCompletionException("AI_BINDING_NOT_FOUND", "No model is bound to this task."));
        var mediaId = aiStore.Documents.Single().MediaId;

        var (_, created) = await SendAsync(Request(HttpMethod.Post, Path(ApiRoutes.Courses.LessonAiJobsTemplate), body: new { jobType = "SYNTHESIZE_LESSON", mediaIds = new[] { mediaId } }));
        await RunWorkerUntilIdleAsync();
        var (_, detail) = await SendAsync(Request(HttpMethod.Get, Path(ApiRoutes.Courses.LessonAiJobByIdTemplate, created.RootElement.GetProperty("data").GetProperty("id").GetGuid())));

        var job = detail.RootElement.GetProperty("data").GetProperty("job");
        Assert.Multiple(() =>
        {
            Assert.That(job.GetProperty("status").GetString(), Is.EqualTo("FAILED"));
            Assert.That(job.GetProperty("errorCode").GetString(), Is.EqualTo("AI_BINDING_NOT_FOUND"));
            Assert.That(lessons.Content, Is.Null);
        });
    }

    [Test]
    public async Task CreateJobReturnsConflictWhenOverwriteIsNotConfirmed()
    {
        lessons.SetContent("Nội dung có sẵn");
        var mediaId = aiStore.AddExtracted(quizzes.LessonId, "a.txt", "x").MediaId;

        var (response, json) = await SendAsync(Request(HttpMethod.Post, Path(ApiRoutes.Courses.LessonAiJobsTemplate), body: new { jobType = "SYNTHESIZE_LESSON", mediaIds = new[] { mediaId } }));

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(ErrorCode(json), Is.EqualTo("LESSON_CONTENT_EXISTS"));
            Assert.That(aiStore.Jobs, Is.Empty);
        });
    }

    [Test]
    public async Task CreateJobReturnsConflictForDocumentsThatAreNotExtracted()
    {
        var (response, json) = await SendAsync(Request(HttpMethod.Post, Path(ApiRoutes.Courses.LessonAiJobsTemplate), body: new { jobType = "GENERATE_QUIZ", mediaIds = new[] { Guid.NewGuid() } }));

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(ErrorCode(json), Is.EqualTo("LESSON_AI_DOCUMENT_NOT_READY"));
        });
    }

    [Test]
    public async Task CreateJobValidatesTheRequestBody()
    {
        var (response, json) = await SendAsync(Request(HttpMethod.Post, Path(ApiRoutes.Courses.LessonAiJobsTemplate), body: new { jobType = "NOPE", mediaIds = Array.Empty<Guid>() }));

        var fields = json.RootElement.GetProperty("error").GetProperty("details").EnumerateArray().Select(detail => detail.GetProperty("field").GetString()).ToArray();
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(ErrorCode(json), Is.EqualTo("VALIDATION_FAILED"));
            Assert.That(fields, Is.EquivalentTo(ExpectedInvalidFields));
        });
    }

    [Test]
    public async Task SecondActiveJobOfTheSameTypeIsRejected()
    {
        var mediaId = aiStore.AddExtracted(quizzes.LessonId, "a.txt", "x").MediaId;
        var body = new { jobType = "GENERATE_QUIZ", mediaIds = new[] { mediaId } };
        await SendAsync(Request(HttpMethod.Post, Path(ApiRoutes.Courses.LessonAiJobsTemplate), body: body));

        var (response, json) = await SendAsync(Request(HttpMethod.Post, Path(ApiRoutes.Courses.LessonAiJobsTemplate), body: body));

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(ErrorCode(json), Is.EqualTo("LESSON_AI_JOB_ACTIVE"));
        });
    }

    [Test]
    public async Task UnknownJobAndUnknownLessonReturnNotFound()
    {
        var (job, jobJson) = await SendAsync(Request(HttpMethod.Get, Path(ApiRoutes.Courses.LessonAiJobByIdTemplate, Guid.NewGuid())));
        var (lesson, lessonJson) = await SendAsync(Request(HttpMethod.Get, Path(ApiRoutes.Courses.LessonAiDocumentsTemplate).Replace(quizzes.LessonId.ToString(), Guid.NewGuid().ToString(), StringComparison.Ordinal)));

        Assert.Multiple(() =>
        {
            Assert.That(job.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(ErrorCode(jobJson), Is.EqualTo("LESSON_AI_JOB_NOT_FOUND"));
            Assert.That(lesson.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(ErrorCode(lessonJson), Is.EqualTo("LESSON_NOT_FOUND"));
        });
    }

    [Test]
    public async Task InvalidRouteIdsReturnValidationFailed()
    {
        var (response, json) = await SendAsync(Request(HttpMethod.Get, "/api/courses/not-a-guid/lessons/also-bad/ai-documents"));

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(ErrorCode(json), Is.EqualTo("VALIDATION_FAILED"));
        });
    }

    [Test]
    public async Task EveryEndpointRequiresAnAdminActor()
    {
        var jobId = Guid.NewGuid();
        var calls = new (HttpMethod Method, string Path)[]
        {
            (HttpMethod.Get, Path(ApiRoutes.Courses.LessonAiDocumentsTemplate)),
            (HttpMethod.Post, Path(ApiRoutes.Courses.LessonAiDocumentsSyncTemplate)),
            (HttpMethod.Post, Path(ApiRoutes.Courses.LessonAiJobsTemplate)),
            (HttpMethod.Get, Path(ApiRoutes.Courses.LessonAiJobsTemplate)),
            (HttpMethod.Get, Path(ApiRoutes.Courses.LessonAiJobByIdTemplate, jobId)),
        };

        foreach (var (method, path) in calls)
        {
            var body = method == HttpMethod.Post && path.EndsWith("ai-jobs", StringComparison.Ordinal) ? new { jobType = "GENERATE_QUIZ", mediaIds = new[] { jobId } } : null;
            var (student, _) = await SendAsync(Request(method, path, ActorHeaderTypes.Student, StudentId, body));
            var (anonymous, _) = await SendAsync(Request(method, path, null, null, body));

            Assert.Multiple(() =>
            {
                Assert.That(student.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), $"{method} {path} as student");
                Assert.That(anonymous.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), $"{method} {path} without actor headers");
            });
        }

        Assert.That(aiStore.Jobs, Is.Empty);
    }

    /// <summary>Cho phép đổi kịch bản model sau khi host đã build, vì worker lấy dependency theo từng scope.</summary>
    private sealed class SwitchableAiCompletionClient(IAiCompletionClient current) : IAiCompletionClient
    {
        public IAiCompletionClient Current { get; set; } = current;

        public Task<AiCompletion> CompleteAsync(string task, IReadOnlyList<AiMessage> messages, IReadOnlyList<AiToolDefinition> tools, Guid actorId, CancellationToken cancellationToken) =>
            Current.CompleteAsync(task, messages, tools, actorId, cancellationToken);
    }
}
