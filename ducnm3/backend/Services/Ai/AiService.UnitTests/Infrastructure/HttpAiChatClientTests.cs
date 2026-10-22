using System.Net;
using System.Text;
using System.Text.Json;
using AiService.Application.Services;
using AiService.Infrastructure.Probe;

namespace AiService.UnitTests.Infrastructure;

public sealed class HttpAiChatClientTests
{
    private const string TextResponse = """
        {"choices":[{"message":{"role":"assistant","content":"Nội dung"},"finish_reason":"stop"}],"usage":{"prompt_tokens":10,"completion_tokens":4}}
        """;

    [Test]
    public async Task SendsBearerKeyToChatCompletionsWithModelMessagesAndTools()
    {
        var handler = new StubHandler(_ => Json(HttpStatusCode.OK, TextResponse));
        var client = new HttpAiChatClient(new HttpClient(handler));

        var result = await client.CompleteAsync(Request(tools: [new AiChatTool("create_quiz_draft", "Tạo quiz", Schema())]), CancellationToken.None);

        using var body = JsonDocument.Parse(handler.Bodies.Single());
        var root = body.RootElement;
        Assert.Multiple(() =>
        {
            Assert.That(handler.Requests.Single().RequestUri?.ToString(), Is.EqualTo("https://api.example.test/v1/chat/completions"));
            Assert.That(handler.Requests.Single().Headers.Authorization?.ToString(), Is.EqualTo("Bearer sk-test"));
            Assert.That(handler.Requests.Single().Headers.Contains("x-api-key"), Is.False);
            Assert.That(root.GetProperty("model").GetString(), Is.EqualTo("gpt-4o-mini"));
            Assert.That(root.GetProperty("messages")[0].GetProperty("content").GetString(), Is.EqualTo("Soạn quiz"));
            Assert.That(root.GetProperty("tools")[0].GetProperty("function").GetProperty("name").GetString(), Is.EqualTo("create_quiz_draft"));
            Assert.That(root.TryGetProperty("temperature", out _), Is.False);
            Assert.That(root.TryGetProperty("max_tokens", out _), Is.False);
            Assert.That(result.Text, Is.EqualTo("Nội dung"));
            Assert.That(result.PromptTokens, Is.EqualTo(10));
            Assert.That(result.CompletionTokens, Is.EqualTo(4));
            Assert.That(result.FinishReason, Is.EqualTo("stop"));
        });
    }

    [Test]
    public async Task OmitsToolsPropertyWhenThereAreNoTools()
    {
        var handler = new StubHandler(_ => Json(HttpStatusCode.OK, TextResponse));

        await new HttpAiChatClient(new HttpClient(handler)).CompleteAsync(Request(), CancellationToken.None);

        using var body = JsonDocument.Parse(handler.Bodies.Single());
        Assert.That(body.RootElement.TryGetProperty("tools", out _), Is.False);
    }

    [Test]
    public async Task AnthropicAlsoSendsNativeHeaders()
    {
        var handler = new StubHandler(_ => Json(HttpStatusCode.OK, TextResponse));

        await new HttpAiChatClient(new HttpClient(handler)).CompleteAsync(Request(providerType: "ANTHROPIC"), CancellationToken.None);

        var request = handler.Requests.Single();
        Assert.Multiple(() =>
        {
            Assert.That(request.Headers.GetValues("x-api-key").Single(), Is.EqualTo("sk-test"));
            Assert.That(request.Headers.Contains("anthropic-version"), Is.True);
        });
    }

    [Test]
    public async Task ImageFilePartsAndToolMessagesAreSerializedInOpenAiShape()
    {
        var handler = new StubHandler(_ => Json(HttpStatusCode.OK, TextResponse));
        var messages = new[]
        {
            new AiChatMessage("user", null,
                [
                    new AiChatContentPart("text", "Đọc tài liệu", null, null, null),
                    new AiChatContentPart("image", null, "image/png", "QUJD", null),
                    new AiChatContentPart("file", null, "application/pdf", "UERG", "bai-giang.pdf"),
                ], null, null),
            new AiChatMessage("assistant", null, null, [new AiChatToolCall("call_1", "get_lesson", "{\"id\":1}")], null),
            new AiChatMessage("tool", "{\"ok\":true}", null, null, "call_1"),
        };

        await new HttpAiChatClient(new HttpClient(handler)).CompleteAsync(Request(messages: messages), CancellationToken.None);

        using var body = JsonDocument.Parse(handler.Bodies.Single());
        var sent = body.RootElement.GetProperty("messages");
        var parts = sent[0].GetProperty("content");
        var assistant = sent[1];
        var tool = sent[2];
        Assert.Multiple(() =>
        {
            Assert.That(parts[0].GetProperty("type").GetString(), Is.EqualTo("text"));
            Assert.That(parts[1].GetProperty("image_url").GetProperty("url").GetString(), Is.EqualTo("data:image/png;base64,QUJD"));
            Assert.That(parts[2].GetProperty("file").GetProperty("filename").GetString(), Is.EqualTo("bai-giang.pdf"));
            Assert.That(parts[2].GetProperty("file").GetProperty("file_data").GetString(), Is.EqualTo("data:application/pdf;base64,UERG"));
            Assert.That(assistant.GetProperty("content").ValueKind, Is.EqualTo(JsonValueKind.Null));
            Assert.That(assistant.GetProperty("tool_calls")[0].GetProperty("function").GetProperty("arguments").GetString(), Is.EqualTo("{\"id\":1}"));
            Assert.That(tool.GetProperty("tool_call_id").GetString(), Is.EqualTo("call_1"));
            Assert.That(tool.GetProperty("content").GetString(), Is.EqualTo("{\"ok\":true}"));
        });
    }

    [Test]
    public async Task GeminiReceivesPdfAsImageUrlDataUriBecauseItRejectsFileParts()
    {
        var handler = new StubHandler(_ => Json(HttpStatusCode.OK, TextResponse));
        var messages = new[]
        {
            new AiChatMessage("user", null,
                [
                    new AiChatContentPart("text", "Đọc tài liệu", null, null, null),
                    new AiChatContentPart("file", null, "application/pdf", "UERG", "bai-giang.pdf"),
                ], null, null),
        };

        await new HttpAiChatClient(new HttpClient(handler)).CompleteAsync(Request(providerType: "GEMINI", messages: messages), CancellationToken.None);

        using var body = JsonDocument.Parse(handler.Bodies.Single());
        var pdf = body.RootElement.GetProperty("messages")[0].GetProperty("content")[1];
        Assert.Multiple(() =>
        {
            Assert.That(pdf.GetProperty("type").GetString(), Is.EqualTo("image_url"));
            Assert.That(pdf.GetProperty("image_url").GetProperty("url").GetString(), Is.EqualTo("data:application/pdf;base64,UERG"));
            Assert.That(pdf.TryGetProperty("file", out _), Is.False);
        });
    }

    [Test]
    public async Task ToolCallsAreParsedAndEmptyIdsAreReplaced()
    {
        const string response = """
            {"choices":[{"message":{"role":"assistant","content":null,"tool_calls":[
              {"id":"call_abc","type":"function","function":{"name":"create_quiz_draft","arguments":"{\"title\":\"A\"}"}},
              {"id":"","type":"function","function":{"name":"update_lesson_content","arguments":{"markdown":"# Hi"}}}
            ]},"finish_reason":"tool_calls"}]}
            """;
        var client = new HttpAiChatClient(new HttpClient(new StubHandler(_ => Json(HttpStatusCode.OK, response))));

        var result = await client.CompleteAsync(Request(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Text, Is.Null);
            Assert.That(result.ToolCalls, Has.Count.EqualTo(2));
            Assert.That(result.ToolCalls[0].Id, Is.EqualTo("call_abc"));
            Assert.That(result.ToolCalls[0].ArgumentsJson, Is.EqualTo("{\"title\":\"A\"}"));
            Assert.That(result.ToolCalls[1].Id, Is.EqualTo("call_2"));
            Assert.That(result.ToolCalls[1].ArgumentsJson, Is.EqualTo("{\"markdown\":\"# Hi\"}"));
            Assert.That(result.FinishReason, Is.EqualTo("tool_calls"));
            Assert.That(result.PromptTokens, Is.Zero);
        });
    }

    [TestCase(HttpStatusCode.Unauthorized, AiChatFailureKind.AuthFailed)]
    [TestCase(HttpStatusCode.Forbidden, AiChatFailureKind.AuthFailed)]
    [TestCase(HttpStatusCode.TooManyRequests, AiChatFailureKind.RateLimited)]
    [TestCase(HttpStatusCode.BadRequest, AiChatFailureKind.Rejected)]
    [TestCase(HttpStatusCode.RequestEntityTooLarge, AiChatFailureKind.Rejected)]
    [TestCase(HttpStatusCode.InternalServerError, AiChatFailureKind.InvalidResponse)]
    public void HttpFailureIsClassified(HttpStatusCode status, AiChatFailureKind expected)
    {
        var client = new HttpAiChatClient(new HttpClient(new StubHandler(_ => Json(status, "{}"))));

        var exception = Assert.ThrowsAsync<AiChatException>(() => client.CompleteAsync(Request(), CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(exception!.Kind, Is.EqualTo(expected));
            Assert.That(exception.HttpStatus, Is.EqualTo((int)status));
        });
    }

    [Test]
    public void RejectedRequestKeepsAShortProviderMessageButMasksTheKey()
    {
        var body = "{\"error\":{\"message\":\"Model does not support tools. key=sk-test " + new string('x', 600) + "\"}}";
        var client = new HttpAiChatClient(new HttpClient(new StubHandler(_ => Json(HttpStatusCode.BadRequest, body))));

        var exception = Assert.ThrowsAsync<AiChatException>(() => client.CompleteAsync(Request(), CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(exception!.Message, Does.Contain("Model does not support tools"));
            Assert.That(exception.Message, Does.Not.Contain("sk-test"));
            Assert.That(exception.Message.Length, Is.LessThan(420));
        });
    }

    [TestCase("not json")]
    [TestCase("{\"choices\":[]}")]
    [TestCase("{}")]
    public void UnreadableSuccessBodyIsInvalidResponse(string body)
    {
        var client = new HttpAiChatClient(new HttpClient(new StubHandler(_ => Json(HttpStatusCode.OK, body))));

        var exception = Assert.ThrowsAsync<AiChatException>(() => client.CompleteAsync(Request(), CancellationToken.None));

        Assert.That(exception!.Kind, Is.EqualTo(AiChatFailureKind.InvalidResponse));
    }

    [Test]
    public void NetworkFailureIsUnreachableAndDoesNotLeakTheException()
    {
        var client = new HttpAiChatClient(new HttpClient(new StubHandler(_ => throw new HttpRequestException("secret-internal-host:5432 refused"))));

        var exception = Assert.ThrowsAsync<AiChatException>(() => client.CompleteAsync(Request(), CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(exception!.Kind, Is.EqualTo(AiChatFailureKind.Unreachable));
            Assert.That(exception.Message, Does.Not.Contain("secret-internal-host"));
        });
    }

    [Test]
    public void InvalidBaseUrlFailsWithoutCallingOut()
    {
        var handler = new StubHandler(_ => Json(HttpStatusCode.OK, TextResponse));
        var client = new HttpAiChatClient(new HttpClient(handler));

        var exception = Assert.ThrowsAsync<AiChatException>(() => client.CompleteAsync(Request(baseUrl: "ftp://api.example.test"), CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(exception!.Kind, Is.EqualTo(AiChatFailureKind.Unreachable));
            Assert.That(handler.Requests, Is.Empty);
        });
    }

    [Test]
    public void CallerCancellationPropagates()
    {
        using var cts = new CancellationTokenSource();
        var client = new HttpAiChatClient(new HttpClient(new StubHandler(_ =>
        {
            cts.Cancel();
            cts.Token.ThrowIfCancellationRequested();
            return Json(HttpStatusCode.OK, TextResponse);
        })));

        Assert.ThrowsAsync<TaskCanceledException>(() => client.CompleteAsync(Request(), cts.Token));
    }

    private static AiChatRequest Request(
        string providerType = "OPENAI",
        string baseUrl = "https://api.example.test/v1",
        IReadOnlyList<AiChatMessage>? messages = null,
        IReadOnlyList<AiChatTool>? tools = null) =>
        new(providerType, baseUrl, "sk-test", "gpt-4o-mini", messages ?? [new AiChatMessage("user", "Soạn quiz", null, null, null)], tools ?? []);

    private static JsonElement Schema() => JsonDocument.Parse("""{"type":"object","properties":{}}""").RootElement.Clone();

    private static Task<HttpResponseMessage> Json(HttpStatusCode status, string body) =>
        Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") });

    private sealed class StubHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = [];

        public List<string> Bodies { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            if (request.Content is not null) Bodies.Add(await request.Content.ReadAsStringAsync(cancellationToken));
            return await send(request);
        }
    }
}
