using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AiService.Api.Endpoints.Completions.CreateCompletion;
using AiService.Application;
using AiService.Application.Common.Errors;
using AiService.Application.Repositories;
using AiService.Application.Services;
using AiService.ComponentTests.Support;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace AiService.ComponentTests.Endpoints;

public sealed class AiCompletionEndpointComponentTests
{
    private const string Task = "quiz-authoring";
    private static readonly string[] ExpectedInvalidFields = ["task", "messages"];
    private static readonly Guid AdminId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    private WebApplication app = null!;
    private HttpClient client = null!;
    private InMemoryAiStore store = null!;
    private FakeAiChatClient chat = null!;
    private InMemoryAiInvocationRepository invocations = null!;

    [SetUp]
    public async System.Threading.Tasks.Task SetUpAsync()
    {
        store = new InMemoryAiStore();
        chat = new FakeAiChatClient();
        invocations = new InMemoryAiInvocationRepository();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton<IAiProviderRepository>(store.ProviderRepository);
        builder.Services.AddSingleton<IAiModelRepository>(store.ModelRepository);
        builder.Services.AddSingleton<IAiServiceBindingRepository>(store.BindingRepository);
        builder.Services.AddSingleton<ISecretProtector, FakeSecretProtector>();
        builder.Services.AddSingleton<IAiChatClient>(chat);
        builder.Services.AddSingleton<IAiInvocationRepository>(invocations);
        builder.Services.AddAiApplication();
        app = builder.Build();
        app.UseSharedApiMiddleware();
        app.MapCreateAiCompletion();
        await app.StartAsync();
        client = app.GetTestClient();
    }

    [TearDown]
    public async System.Threading.Tasks.Task TearDownAsync()
    {
        client.Dispose();
        await app.DisposeAsync();
    }

    [Test]
    public async System.Threading.Tasks.Task CompletionReturnsToolCallsAndUsageWithoutLeakingTheKey()
    {
        Bind(Task);
        chat.Result = new AiChatResult(null, [new AiChatToolCall("call_1", "create_quiz_draft", "{\"title\":\"Quiz\"}")], "tool_calls", 200, 60);

        using var response = await SendAsync(new
        {
            task = Task,
            messages = new object[]
            {
                new { role = "system", text = "Bạn là giáo viên." },
                new { role = "user", parts = new object[] { new { type = "text", text = "Nội dung bài học" } } },
            },
            tools = new[]
            {
                new { name = "create_quiz_draft", description = "Tạo quiz nháp", parametersSchema = new { type = "object", properties = new { title = new { type = "string" } } } },
            },
        });
        var body = await response.Content.ReadAsStringAsync(TestContext.CurrentContext.CancellationToken);
        using var document = JsonDocument.Parse(body);
        var data = document.RootElement.GetProperty("data");

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Headers.CacheControl?.NoStore, Is.True);
            Assert.That(data.GetProperty("task").GetString(), Is.EqualTo(Task));
            Assert.That(data.GetProperty("modelName").GetString(), Is.EqualTo("gpt-4o-mini"));
            Assert.That(data.GetProperty("toolCalls")[0].GetProperty("name").GetString(), Is.EqualTo("create_quiz_draft"));
            Assert.That(data.GetProperty("toolCalls")[0].GetProperty("argumentsJson").GetString(), Is.EqualTo("{\"title\":\"Quiz\"}"));
            Assert.That(data.GetProperty("promptTokens").GetInt32(), Is.EqualTo(200));
            Assert.That(data.GetProperty("finishReason").GetString(), Is.EqualTo("tool_calls"));
            Assert.That(body, Does.Not.Contain("sk-original"));
            Assert.That(chat.Requests.Single().Tools.Single().Name, Is.EqualTo("create_quiz_draft"));
            Assert.That(chat.Requests.Single().Messages[1].Parts!.Single().Text, Is.EqualTo("Nội dung bài học"));
            Assert.That(invocations.Records.Single().Status, Is.EqualTo("SUCCESS"));
        });
    }

    [Test]
    public async System.Threading.Tasks.Task MissingBindingReturnsConflictEnvelope()
    {
        using var response = await SendAsync(new { task = Task, messages = new[] { new { role = "user", text = "hi" } } });
        var envelope = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(envelope?.Error.Code, Is.EqualTo(AiErrorCodes.TaskNotConfigured));
            Assert.That(chat.Requests, Is.Empty);
        });
    }

    [Test]
    public async System.Threading.Tasks.Task InvalidPayloadReturnsValidationDetails()
    {
        using var response = await SendAsync(new { task = "nope", messages = Array.Empty<object>() });
        var envelope = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(envelope?.Error.Code, Is.EqualTo(ApiErrorCodes.ValidationFailed));
            Assert.That(envelope?.Error.Details?.Select(detail => detail.Field), Is.EquivalentTo(ExpectedInvalidFields));
        });
    }

    [TestCase(AiChatFailureKind.RateLimited, HttpStatusCode.TooManyRequests)]
    [TestCase(AiChatFailureKind.Timeout, HttpStatusCode.GatewayTimeout)]
    [TestCase(AiChatFailureKind.AuthFailed, HttpStatusCode.BadGateway)]
    public async System.Threading.Tasks.Task ProviderFailureMapsToSafeStatus(AiChatFailureKind kind, HttpStatusCode expected)
    {
        Bind(Task);
        chat.Failure = new AiChatException(kind, null, "internal detail");

        using var response = await SendAsync(new { task = Task, messages = new[] { new { role = "user", text = "hi" } } });
        var body = await response.Content.ReadAsStringAsync(TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(expected));
            Assert.That(body, Does.Not.Contain("internal detail").And.Not.Contain("sk-original"));
            Assert.That(invocations.Records.Single().Status, Is.EqualTo("FAILED"));
        });
    }

    [Test]
    public async System.Threading.Tasks.Task StudentActorIsForbidden()
    {
        Bind(Task);

        using var response = await SendAsync(new { task = Task, messages = new[] { new { role = "user", text = "hi" } } }, ActorHeaderTypes.Student);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            Assert.That(chat.Requests, Is.Empty);
        });
    }

    private void Bind(string task)
    {
        var provider = store.AddProvider();
        var model = store.AddModel(provider, "gpt-4o-mini");
        store.BindingRepository.UpsertAsync(task, model.Id, CancellationToken.None).GetAwaiter().GetResult();
    }

    private Task<HttpResponseMessage> SendAsync(object body, string actorType = ActorHeaderTypes.Admin)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, ApiRoutes.Ai.CompletionsServicePath())
        {
            Content = JsonContent.Create(body),
        };
        request.Headers.Add(ApiHeaderNames.ActorType, actorType);
        request.Headers.Add(ApiHeaderNames.ActorId, AdminId.ToString());
        return client.SendAsync(request, TestContext.CurrentContext.CancellationToken);
    }
}
