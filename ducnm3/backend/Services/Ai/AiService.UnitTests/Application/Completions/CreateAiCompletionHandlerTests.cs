using System.Text.Json;
using AiService.Application.Common.Errors;
using AiService.Application.Services;
using AiService.Application.UseCases.Completions.Create;
using AiService.UnitTests.Support;
using Microsoft.Extensions.Logging.Abstractions;

namespace AiService.UnitTests.Application.Completions;

public sealed class CreateAiCompletionHandlerTests
{
    private const string Task = "quiz-authoring";

    private InMemoryAiStore _store = null!;
    private FakeAiChatClient _client = null!;
    private InMemoryAiInvocationRepository _invocations = null!;
    private CreateAiCompletionHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _store = new InMemoryAiStore();
        _client = new FakeAiChatClient();
        _invocations = new InMemoryAiInvocationRepository();
        _handler = new CreateAiCompletionHandler(
            _store.BindingRepository,
            _store.ProviderRepository,
            new FakeSecretProtector(),
            _client,
            _invocations,
            NullLogger<CreateAiCompletionHandler>.Instance);
    }

    [Test]
    public async Task BoundTaskCallsTheBoundModelWithTheDecryptedKeyAndLogsTheInvocation()
    {
        var model = BindTask(Task);
        _client.Result = new AiChatResult(null, [new AiChatToolCall("call_1", "create_quiz_draft", "{\"title\":\"A\"}")], "tool_calls", 120, 45);

        var result = await _handler.HandleAsync(Command(Task), CancellationToken.None);

        var request = _client.Requests.Single();
        var record = _invocations.Records.Single();
        Assert.Multiple(() =>
        {
            Assert.That(request.ApiKey, Is.EqualTo("sk-original-abcd"));
            Assert.That(request.ModelName, Is.EqualTo(model.ModelName));
            Assert.That(request.BaseUrl, Is.EqualTo("https://api.openai.com/v1"));
            Assert.That(request.Tools.Single().Name, Is.EqualTo("create_quiz_draft"));
            Assert.That(result.ToolCalls.Single().Name, Is.EqualTo("create_quiz_draft"));
            Assert.That(result.PromptTokens, Is.EqualTo(120));
            Assert.That(record.Status, Is.EqualTo("SUCCESS"));
            Assert.That(record.TaskName, Is.EqualTo(Task));
            Assert.That(record.ToolCallCount, Is.EqualTo(1));
            Assert.That(record.CompletionTokens, Is.EqualTo(45));
        });
    }

    [Test]
    public void TaskWithoutBindingIsAConflictAndDoesNotCallTheProvider()
    {
        var exception = Assert.ThrowsAsync<AiApplicationException>(() => _handler.HandleAsync(Command(Task), CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(exception!.ErrorCode, Is.EqualTo(AiErrorCodes.TaskNotConfigured));
            Assert.That(exception.StatusCode, Is.EqualTo(409));
            Assert.That(_client.Requests, Is.Empty);
        });
    }

    [Test]
    public void InactiveModelIsRejected()
    {
        BindTask(Task, modelStatus: "INACTIVE");

        var exception = Assert.ThrowsAsync<AiApplicationException>(() => _handler.HandleAsync(Command(Task), CancellationToken.None));

        Assert.That(exception!.ErrorCode, Is.EqualTo(AiErrorCodes.ReferenceInactive));
    }

    [TestCase(null)]
    [TestCase("unknown-task")]
    public void UnsupportedTaskFailsValidation(string? task)
    {
        var exception = Assert.ThrowsAsync<AiApplicationException>(() => _handler.HandleAsync(Command(task), CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(exception!.StatusCode, Is.EqualTo(400));
            Assert.That(exception.Details!.Select(detail => detail.Field), Does.Contain("task"));
        });
    }

    [Test]
    public void MalformedMessagesAndToolsAreListedTogether()
    {
        BindTask(Task);
        var command = new CreateAiCompletionCommand(
            Task,
            [
                new AiChatMessage("robot", "hi", null, null, null),
                new AiChatMessage("user", "text", [new AiChatContentPart("text", "both", null, null, null)], null, null),
                new AiChatMessage("tool", "result", null, null, null),
                new AiChatMessage("user", null, [new AiChatContentPart("image", null, "application/pdf", "AAAA", null)], null, null),
            ],
            [
                new AiChatTool("bad name!", "desc", Schema()),
                new AiChatTool("ok", "", Schema()),
            ]);

        var exception = Assert.ThrowsAsync<AiApplicationException>(() => _handler.HandleAsync(command, CancellationToken.None));

        var fields = exception!.Details!.Select(detail => detail.Field).ToArray();
        Assert.Multiple(() =>
        {
            Assert.That(fields, Does.Contain("messages[0].role"));
            Assert.That(fields, Does.Contain("messages[1]"));
            Assert.That(fields, Does.Contain("messages[2].toolCallId"));
            Assert.That(fields, Does.Contain("messages[3].parts[0].mediaType"));
            Assert.That(fields, Does.Contain("tools[0].name"));
            Assert.That(fields, Does.Contain("tools[1].description"));
            Assert.That(_client.Requests, Is.Empty);
        });
    }

    [TestCase(AiChatFailureKind.AuthFailed, AiErrorCodes.ProviderAuthFailed, 502)]
    [TestCase(AiChatFailureKind.RateLimited, AiErrorCodes.ProviderRateLimited, 429)]
    [TestCase(AiChatFailureKind.Timeout, AiErrorCodes.ProviderTimeout, 504)]
    [TestCase(AiChatFailureKind.Unreachable, AiErrorCodes.ProviderUnreachable, 502)]
    [TestCase(AiChatFailureKind.Rejected, AiErrorCodes.ProviderRejectedRequest, 502)]
    [TestCase(AiChatFailureKind.InvalidResponse, AiErrorCodes.ProviderInvalidResponse, 502)]
    public void ProviderFailureIsMappedAndLoggedAsFailed(AiChatFailureKind kind, string expectedCode, int expectedStatus)
    {
        BindTask(Task);
        _client.Failure = new AiChatException(kind, null, "provider said no");

        var exception = Assert.ThrowsAsync<AiApplicationException>(() => _handler.HandleAsync(Command(Task), CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(exception!.ErrorCode, Is.EqualTo(expectedCode));
            Assert.That(exception.StatusCode, Is.EqualTo(expectedStatus));
            Assert.That(_invocations.Records.Single().Status, Is.EqualTo("FAILED"));
            Assert.That(_invocations.Records.Single().ErrorCode, Is.EqualTo(expectedCode));
        });
    }

    [Test]
    public async Task FailureToWriteTheLogDoesNotLoseTheModelResult()
    {
        BindTask(Task);
        _invocations.Throws = true;

        var result = await _handler.HandleAsync(Command(Task), CancellationToken.None);

        Assert.That(result.Text, Is.EqualTo("Xin chào"));
    }

    private AiService.Application.Repositories.AiModelRecord BindTask(string task, string modelStatus = "ACTIVE")
    {
        var provider = _store.AddProvider();
        var model = _store.AddModel(provider, "gpt-4o-mini", modelStatus);
        _store.BindingRepository.UpsertAsync(task, model.Id, CancellationToken.None).GetAwaiter().GetResult();
        return model;
    }

    private static CreateAiCompletionCommand Command(string? task) =>
        new(
            task,
            [new AiChatMessage("user", "Soạn quiz", null, null, null)],
            [new AiChatTool("create_quiz_draft", "Tạo quiz nháp", Schema())]);

    private static JsonElement Schema() => JsonDocument.Parse("""{"type":"object","properties":{}}""").RootElement.Clone();
}
