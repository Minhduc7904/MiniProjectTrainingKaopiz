using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging.Abstractions;
using BuildingBlocks.Presentation.Extensions;
using CourseService.Api.Contracts.Quizzes;
using CourseService.Api.Endpoints.Learning;
using CourseService.Api.Endpoints.Quizzes.CreateQuiz;
using CourseService.Api.Endpoints.Quizzes.DeleteQuiz;
using CourseService.Api.Endpoints.Quizzes.GetQuizDetail;
using CourseService.Api.Endpoints.Quizzes.GetQuizzes;
using CourseService.Api.Endpoints.Quizzes.UpdateQuiz;
using CourseService.Application;
using CourseService.Application.Repositories;
using CourseService.Application.Services.Media;
using CourseService.Application.UseCases.Learning;
using CourseService.ComponentTests.Support;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace CourseService.ComponentTests.Endpoints;

public sealed class QuizEndpointsComponentTests
{
    private static readonly Guid AdminId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid StudentId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    private WebApplication app = null!;
    private HttpClient client = null!;
    private InMemoryQuizStore store = null!;
    private RecordingCommandSender commandSender = null!;
    private HashSet<Guid> enrolled = null!;

    [SetUp]
    public async Task SetUpAsync()
    {
        store = new InMemoryQuizStore();
        enrolled = [StudentId];
        commandSender = new RecordingCommandSender();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton<IQuizCommandRepository>(store);
        builder.Services.AddSingleton<IStudentQuizRepository>(store);
        builder.Services.AddSingleton<IStudentLearningRepository>(new StubEnrollment(store.CourseId, enrolled));
        builder.Services.AddSingleton<ILessonCommandRepository>(new StubLessonRepository(store.CourseId, store.LessonId));
        builder.Services.AddSingleton<ICourseMediaReader>(new StubCourseMediaReader());
        builder.Services.AddSingleton<ICommandSender>(commandSender);
        builder.Services.AddCourseApplication();
        app = builder.Build();
        app.UseSharedApiMiddleware();
        app.MapCreateQuiz();
        app.MapGetQuizzes();
        app.MapGetQuizDetail();
        app.MapUpdateQuiz();
        app.MapDeleteQuiz();
        app.MapGetStudentQuizzes();
        app.MapGetStudentQuizDetail();
        app.MapStartQuizAttempt();
        app.MapSaveQuizAnswers();
        app.MapSubmitQuizAttempt();
        app.MapGetQuizAttempts();
        app.MapGetQuizAttemptDetail();
        await app.StartAsync();
        client = app.GetTestClient();
    }

    [TearDown]
    public async Task TearDownAsync()
    {
        client.Dispose();
        await app.DisposeAsync();
    }

    private static CreateQuizRequest ValidQuiz(string status = "PUBLISHED") =>
        new(
            "Kiểm tra cuối bài",
            "Mô tả có **đậm**",
            status,
            [
                new("Thủ đô của Việt Nam?", "SINGLE_CHOICE", 2m, [new("Hà Nội", true), new("Huế", false)]),
                new("Số nguyên tố nào?", "MULTIPLE_CHOICE", 3m, [new("2", true), new("3", true), new("4", false)]),
            ]);

    private string AdminQuizzesPath() =>
        ApiRoutes.Courses.QuizzesTemplate.Replace("{courseId}", store.CourseId.ToString()).Replace("{lessonId}", store.LessonId.ToString());

    private string StudentPath(string template, Guid? quizId = null, Guid? attemptId = null) =>
        template
            .Replace("{courseId}", store.CourseId.ToString())
            .Replace("{lessonId}", store.LessonId.ToString())
            .Replace("{quizId}", quizId?.ToString())
            .Replace("{attemptId}", attemptId?.ToString());

    private static HttpRequestMessage Admin(HttpMethod method, string path, object? body = null) => Build(method, path, ActorHeaderTypes.Admin, AdminId, body);

    private static HttpRequestMessage Student(HttpMethod method, string path, object? body = null, Guid? studentId = null) =>
        Build(method, path, ActorHeaderTypes.Student, studentId ?? StudentId, body);

    private static HttpRequestMessage Build(HttpMethod method, string path, string actorType, Guid actorId, object? body)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Add(ApiHeaderNames.ActorType, actorType);
        request.Headers.Add(ApiHeaderNames.ActorId, actorId.ToString());
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

    private async Task<Guid> CreatePublishedQuizAsync()
    {
        var (response, json) = await SendAsync(Admin(HttpMethod.Post, AdminQuizzesPath(), ValidQuiz()));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        return json.RootElement.GetProperty("data").GetProperty("id").GetGuid();
    }

    [Test]
    public async Task Create_ValidQuiz_ReturnsCreatedWithRenderedHtmlTotalPointsAndLocation()
    {
        var (response, json) = await SendAsync(Admin(HttpMethod.Post, AdminQuizzesPath(), ValidQuiz()));
        var data = json.RootElement.GetProperty("data");

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(response.Headers.Location, Is.Not.Null);
            Assert.That(data.GetProperty("totalPoints").GetDecimal(), Is.EqualTo(5m));
            Assert.That(data.GetProperty("questionCount").GetInt32(), Is.EqualTo(2));
            Assert.That(data.GetProperty("descriptionHtml").GetString(), Does.Contain("<strong>đậm</strong>"));
            Assert.That(data.GetProperty("questions")[0].GetProperty("options")[0].GetProperty("isCorrect").GetBoolean(), Is.True);
        });
    }

    [Test]
    public async Task Create_InvalidQuiz_ReturnsValidationFailedWithFieldDetails()
    {
        var invalid = new CreateQuizRequest("", null, "PUBLISHED", []);

        var (response, json) = await SendAsync(Admin(HttpMethod.Post, AdminQuizzesPath(), invalid));

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(json.RootElement.GetProperty("error").GetProperty("code").GetString(), Is.EqualTo(ApiErrorCodes.ValidationFailed));
            Assert.That(store.Quizzes, Is.Empty);
        });
    }

    [Test]
    public async Task Create_AsStudent_ReturnsForbidden()
    {
        var (response, _) = await SendAsync(Student(HttpMethod.Post, AdminQuizzesPath(), ValidQuiz()));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task Create_UnknownLesson_ReturnsNotFound()
    {
        var path = ApiRoutes.Courses.QuizzesTemplate.Replace("{courseId}", store.CourseId.ToString()).Replace("{lessonId}", Guid.NewGuid().ToString());

        var (response, json) = await SendAsync(Admin(HttpMethod.Post, path, ValidQuiz()));

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(json.RootElement.GetProperty("error").GetProperty("code").GetString(), Is.EqualTo("LESSON_NOT_FOUND"));
        });
    }

    [Test]
    public async Task List_ReturnsSummariesWithCounts()
    {
        await CreatePublishedQuizAsync();

        var (response, json) = await SendAsync(Admin(HttpMethod.Get, AdminQuizzesPath()));

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(json.RootElement.GetProperty("data")[0].GetProperty("questionCount").GetInt32(), Is.EqualTo(2));
            Assert.That(json.RootElement.GetProperty("data")[0].GetProperty("totalPoints").GetDecimal(), Is.EqualTo(5m));
        });
    }

    [Test]
    public async Task Detail_UnknownQuiz_ReturnsNotFound()
    {
        var (response, json) = await SendAsync(Admin(HttpMethod.Get, ApiRoutes.Courses.QuizByIdServicePath(store.CourseId, store.LessonId, Guid.NewGuid())));

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(json.RootElement.GetProperty("error").GetProperty("code").GetString(), Is.EqualTo("QUIZ_NOT_FOUND"));
        });
    }

    [Test]
    public async Task Update_MetadataOnly_KeepsQuestions()
    {
        var quizId = await CreatePublishedQuizAsync();

        var (response, json) = await SendAsync(Admin(
            HttpMethod.Put,
            ApiRoutes.Courses.QuizByIdServicePath(store.CourseId, store.LessonId, quizId),
            new UpdateQuizRequest("Tên mới", null, "DRAFT", null)));

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(json.RootElement.GetProperty("data").GetProperty("title").GetString(), Is.EqualTo("Tên mới"));
            Assert.That(json.RootElement.GetProperty("data").GetProperty("questionCount").GetInt32(), Is.EqualTo(2));
        });
    }

    [Test]
    public async Task Update_ReplacingQuestionsAfterAttempt_ReturnsConflict()
    {
        var quizId = await CreatePublishedQuizAsync();
        await SendAsync(Student(HttpMethod.Post, StudentPath(ApiRoutes.Courses.StudentQuizAttemptsTemplate, quizId)));

        var (response, json) = await SendAsync(Admin(
            HttpMethod.Put,
            ApiRoutes.Courses.QuizByIdServicePath(store.CourseId, store.LessonId, quizId),
            new UpdateQuizRequest("Tên mới", null, "PUBLISHED", ValidQuiz().Questions)));

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(json.RootElement.GetProperty("error").GetProperty("code").GetString(), Is.EqualTo("QUIZ_HAS_ATTEMPTS"));
        });
    }

    [Test]
    public async Task Delete_ExistingQuiz_ReturnsAcceptedAndRemovesIt()
    {
        var quizId = await CreatePublishedQuizAsync();

        var (response, _) = await SendAsync(Admin(HttpMethod.Delete, ApiRoutes.Courses.QuizByIdServicePath(store.CourseId, store.LessonId, quizId)));

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));
            Assert.That(store.Quizzes, Is.Empty);
        });
    }

    [Test]
    public async Task StudentDetail_NeverExposesCorrectAnswersOrMarkdownSource()
    {
        var quizId = await CreatePublishedQuizAsync();

        var (response, json) = await SendAsync(Student(HttpMethod.Get, StudentPath(ApiRoutes.Courses.StudentQuizByIdTemplate, quizId)));
        var raw = json.RootElement.GetRawText();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Headers.CacheControl?.NoStore, Is.True);
            Assert.That(raw, Does.Not.Contain("isCorrect").IgnoreCase);
            Assert.That(raw, Does.Not.Contain("Markdown"));
            Assert.That(json.RootElement.GetProperty("data").GetProperty("questions")[0].GetProperty("questionHtml").GetString(), Does.Contain("Thủ đô"));
        });
    }

    [Test]
    public async Task StudentDetail_DraftQuiz_ReturnsNotFound()
    {
        var (_, created) = await SendAsync(Admin(HttpMethod.Post, AdminQuizzesPath(), ValidQuiz("DRAFT")));
        var quizId = created.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        var (response, _) = await SendAsync(Student(HttpMethod.Get, StudentPath(ApiRoutes.Courses.StudentQuizByIdTemplate, quizId)));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Student_NotEnrolled_ReturnsForbidden()
    {
        var quizId = await CreatePublishedQuizAsync();

        var (response, json) = await SendAsync(Student(HttpMethod.Get, StudentPath(ApiRoutes.Courses.StudentQuizByIdTemplate, quizId), studentId: Guid.NewGuid()));

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            Assert.That(json.RootElement.GetProperty("error").GetProperty("code").GetString(), Is.EqualTo("STUDENT_NOT_ENROLLED"));
        });
    }

    [Test]
    public async Task Student_FullFlow_StartSaveResumeSubmitRetake()
    {
        var quizId = await CreatePublishedQuizAsync();
        var quiz = store.Quizzes[quizId];
        var single = quiz.Questions[0];
        var multiple = quiz.Questions[1];
        var attemptsPath = StudentPath(ApiRoutes.Courses.StudentQuizAttemptsTemplate, quizId);

        var (startResponse, started) = await SendAsync(Student(HttpMethod.Post, attemptsPath));
        var attemptId = started.RootElement.GetProperty("data").GetProperty("id").GetGuid();
        var answersPath = StudentPath(ApiRoutes.Courses.StudentQuizAttemptAnswersTemplate, quizId, attemptId);
        var (saveResponse, _) = await SendAsync(Student(HttpMethod.Put, answersPath, new SaveQuizAnswersRequest([new(single.Id, [single.Options[0].Id])])));
        var (resumeResponse, resumed) = await SendAsync(Student(HttpMethod.Post, attemptsPath));
        var (submitResponse, submitted) = await SendAsync(Student(
            HttpMethod.Post,
            StudentPath(ApiRoutes.Courses.StudentQuizAttemptSubmitTemplate, quizId, attemptId),
            new SubmitQuizAttemptRequest([new(single.Id, [single.Options[0].Id]), new(multiple.Id, [multiple.Options[0].Id, multiple.Options[1].Id])])));
        var (retakeResponse, retake) = await SendAsync(Student(HttpMethod.Post, attemptsPath));
        var (listResponse, list) = await SendAsync(Student(HttpMethod.Get, attemptsPath));

        var resumedAnswers = resumed.RootElement.GetProperty("data").GetProperty("answers");
        var result = submitted.RootElement.GetProperty("data").GetProperty("result");
        Assert.Multiple(() =>
        {
            Assert.That(startResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(saveResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(resumeResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(resumed.RootElement.GetProperty("data").GetProperty("id").GetGuid(), Is.EqualTo(attemptId));
            Assert.That(resumedAnswers[0].GetProperty("optionIds")[0].GetGuid(), Is.EqualTo(single.Options[0].Id));
            Assert.That(submitResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result.GetProperty("score").GetDecimal(), Is.EqualTo(5m));
            Assert.That(result.GetProperty("maxScore").GetDecimal(), Is.EqualTo(5m));
            Assert.That(result.GetProperty("correctCount").GetInt32(), Is.EqualTo(2));
            Assert.That(retakeResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(retake.RootElement.GetProperty("data").GetProperty("id").GetGuid(), Is.Not.EqualTo(attemptId));
            Assert.That(listResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(list.RootElement.GetProperty("data").GetArrayLength(), Is.EqualTo(2));
        });
    }

    [Test]
    public async Task Submit_Twice_ReturnsConflict()
    {
        var quizId = await CreatePublishedQuizAsync();
        var (_, started) = await SendAsync(Student(HttpMethod.Post, StudentPath(ApiRoutes.Courses.StudentQuizAttemptsTemplate, quizId)));
        var attemptId = started.RootElement.GetProperty("data").GetProperty("id").GetGuid();
        var submitPath = StudentPath(ApiRoutes.Courses.StudentQuizAttemptSubmitTemplate, quizId, attemptId);
        await SendAsync(Student(HttpMethod.Post, submitPath, new SubmitQuizAttemptRequest(null)));

        var (response, json) = await SendAsync(Student(HttpMethod.Post, submitPath, new SubmitQuizAttemptRequest(null)));

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(json.RootElement.GetProperty("error").GetProperty("code").GetString(), Is.EqualTo("QUIZ_ATTEMPT_ALREADY_SUBMITTED"));
        });
    }

    [Test]
    public async Task SaveAnswers_ForeignOption_ReturnsValidationFailed()
    {
        var quizId = await CreatePublishedQuizAsync();
        var (_, started) = await SendAsync(Student(HttpMethod.Post, StudentPath(ApiRoutes.Courses.StudentQuizAttemptsTemplate, quizId)));
        var attemptId = started.RootElement.GetProperty("data").GetProperty("id").GetGuid();
        var question = store.Quizzes[quizId].Questions[0];

        var (response, _) = await SendAsync(Student(
            HttpMethod.Put,
            StudentPath(ApiRoutes.Courses.StudentQuizAttemptAnswersTemplate, quizId, attemptId),
            new SaveQuizAnswersRequest([new(question.Id, [Guid.NewGuid()])])));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task AttemptDetail_OfAnotherStudent_ReturnsNotFound()
    {
        var quizId = await CreatePublishedQuizAsync();
        var (_, started) = await SendAsync(Student(HttpMethod.Post, StudentPath(ApiRoutes.Courses.StudentQuizAttemptsTemplate, quizId)));
        var attemptId = started.RootElement.GetProperty("data").GetProperty("id").GetGuid();
        var otherStudent = Guid.NewGuid();
        enrolled.Add(otherStudent);

        var (response, json) = await SendAsync(Student(
            HttpMethod.Get, StudentPath(ApiRoutes.Courses.StudentQuizAttemptByIdTemplate, quizId, attemptId), studentId: otherStudent));

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(json.RootElement.GetProperty("error").GetProperty("code").GetString(), Is.EqualTo("QUIZ_ATTEMPT_NOT_FOUND"));
        });
    }

    private sealed class StubCourseMediaReader : ICourseMediaReader
    {
        public Task<IReadOnlyList<Guid>> GetActiveUsageIdsAsync(IReadOnlyList<CourseMediaUsageOwnerScope> owners, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Guid>>([]);

        public Task<CourseMediaSet> GetAsync(Guid courseId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IReadOnlyDictionary<Guid, CourseMediaSet>> GetManyAsync(IReadOnlyList<Guid> courseIds, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IReadOnlyList<CourseMediaAsset>> GetLessonAttachmentsAsync(Guid lessonId, CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
