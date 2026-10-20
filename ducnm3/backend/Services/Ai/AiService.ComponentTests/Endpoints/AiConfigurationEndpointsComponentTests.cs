using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AiService.Api.Endpoints.Bindings.DeleteBinding;
using AiService.Api.Endpoints.Bindings.GetBinding;
using AiService.Api.Endpoints.Bindings.GetBindings;
using AiService.Api.Endpoints.Bindings.UpsertBinding;
using AiService.Api.Endpoints.Models.CreateModel;
using AiService.Api.Endpoints.Models.DeleteModel;
using AiService.Api.Endpoints.Models.GetModelDetail;
using AiService.Api.Endpoints.Models.GetModels;
using AiService.Api.Endpoints.Models.UpdateModel;
using AiService.Api.Endpoints.Options.GetOptions;
using AiService.Api.Endpoints.Providers.CreateProvider;
using AiService.Api.Endpoints.Providers.DeleteProvider;
using AiService.Api.Endpoints.Providers.GetProviderDetail;
using AiService.Api.Endpoints.Providers.GetProviders;
using AiService.Api.Endpoints.Providers.HealthCheck;
using AiService.Api.Endpoints.Providers.UpdateProvider;
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

public sealed class AiConfigurationEndpointsComponentTests
{
    private const string SecretKey = "sk-super-secret-key-WXYZ";
    private static readonly Guid AdminId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly string[] RequiredInvalidFields = ["name", "providerType", "apiKey"];
    private static readonly string[] ExpectedProviderTypes = ["OPENAI", "GEMINI", "ANTHROPIC", "MISTRAL", "CUSTOM"];

    private WebApplication app = null!;
    private HttpClient client = null!;
    private InMemoryAiStore store = null!;
    private FakeAiProviderProbe probe = null!;

    [SetUp]
    public async Task SetUpAsync()
    {
        store = new InMemoryAiStore();
        probe = new FakeAiProviderProbe();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton<IAiProviderRepository>(store.ProviderRepository);
        builder.Services.AddSingleton<IAiModelRepository>(store.ModelRepository);
        builder.Services.AddSingleton<IAiServiceBindingRepository>(store.BindingRepository);
        builder.Services.AddSingleton<ISecretProtector, FakeSecretProtector>();
        builder.Services.AddSingleton<IAiProviderProbe>(probe);
        builder.Services.AddAiApplication();
        app = builder.Build();
        app.UseSharedApiMiddleware();
        app.MapGetAiOptions();
        app.MapCreateAiProvider();
        app.MapGetAiProviders();
        app.MapGetAiProviderDetail();
        app.MapUpdateAiProvider();
        app.MapDeleteAiProvider();
        app.MapCheckAiProvidersHealth();
        app.MapCheckAiProviderHealth();
        app.MapCreateAiModel();
        app.MapGetAiModels();
        app.MapGetAiModelDetail();
        app.MapUpdateAiModel();
        app.MapDeleteAiModel();
        app.MapGetAiServiceBindings();
        app.MapGetAiServiceBinding();
        app.MapUpsertAiServiceBinding();
        app.MapDeleteAiServiceBinding();
        await app.StartAsync();
        client = app.GetTestClient();
    }

    [TearDown]
    public async Task TearDownAsync()
    {
        client.Dispose();
        await app.DisposeAsync();
    }

    [Test]
    public async Task CreateProviderReturnsCreatedWithHintAndNeverEchoesTheApiKey()
    {
        using var response = await SendAsync(HttpMethod.Post, ApiRoutes.Ai.ProvidersServicePath(), new
        {
            name = "OpenAI main",
            providerType = "OPENAI",
            apiKey = SecretKey,
        });
        var body = await response.Content.ReadAsStringAsync(TestContext.CurrentContext.CancellationToken);
        using var document = JsonDocument.Parse(body);
        var data = document.RootElement.GetProperty("data");
        var id = data.GetProperty("id").GetGuid();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(response.Headers.Location?.OriginalString, Is.EqualTo($"/ai/api/ai/providers/{id:D}"));
            Assert.That(data.GetProperty("apiKeyHint").GetString(), Is.EqualTo("****WXYZ"));
            Assert.That(data.GetProperty("baseUrl").GetString(), Is.EqualTo("https://api.openai.com/v1"));
            Assert.That(body, Does.Not.Contain(SecretKey));
            Assert.That(data.TryGetProperty("apiKey", out _), Is.False);
            Assert.That(data.TryGetProperty("apiKeyEncrypted", out _), Is.False);
            Assert.That(store.Providers[id].ApiKeyEncrypted, Is.EqualTo(FakeSecretProtector.Prefix + SecretKey));
        });
    }

    [Test]
    public async Task HealthChecksReportEveryProviderWithoutLeakingTheKey()
    {
        var online = store.AddProvider("Online");
        var inactive = store.AddProvider("Off", status: "INACTIVE");
        var rejected = store.AddProvider("Rejected");
        store.Providers[rejected.Id] = store.Providers[rejected.Id] with { Record = rejected with { BaseUrl = "https://rejected.example.test/v1" } };
        probe.Setup("https://rejected.example.test/v1", new AiProviderProbeResult("AUTH_FAILED", 401, 90, "Provider rejected the API key."));

        using var response = await SendAsync(HttpMethod.Post, ApiRoutes.Ai.ProviderHealthChecksServicePath());
        var body = await response.Content.ReadAsStringAsync(TestContext.CurrentContext.CancellationToken);
        using var document = JsonDocument.Parse(body);
        var items = document.RootElement.GetProperty("data").EnumerateArray().ToDictionary(item => item.GetProperty("providerId").GetGuid());

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Headers.CacheControl?.NoStore, Is.True);
            Assert.That(items, Has.Count.EqualTo(3));
            Assert.That(items[online.Id].GetProperty("healthStatus").GetString(), Is.EqualTo("ONLINE"));
            Assert.That(items[online.Id].GetProperty("name").GetString(), Is.EqualTo("Online"));
            Assert.That(items[inactive.Id].GetProperty("healthStatus").GetString(), Is.EqualTo("DISABLED"));
            Assert.That(items[rejected.Id].GetProperty("healthStatus").GetString(), Is.EqualTo("AUTH_FAILED"));
            Assert.That(items[rejected.Id].GetProperty("httpStatus").GetInt32(), Is.EqualTo(401));
            Assert.That(body, Does.Not.Contain("sk-original").And.Not.Contain("apiKeyEncrypted"));
        });
    }

    [Test]
    public async Task HealthCheckOfOneProviderReturnsItsStatusOr404()
    {
        var provider = store.AddProvider();

        using var ok = await SendAsync(HttpMethod.Post, ApiRoutes.Ai.ProviderHealthCheckByIdServicePath(provider.Id));
        using var missing = await SendAsync(HttpMethod.Post, ApiRoutes.Ai.ProviderHealthCheckByIdServicePath(Guid.NewGuid()));
        using var student = await SendAsync(HttpMethod.Post, ApiRoutes.Ai.ProviderHealthChecksServicePath(), actorType: ActorHeaderTypes.Student);
        using var document = JsonDocument.Parse(await ok.Content.ReadAsStringAsync(TestContext.CurrentContext.CancellationToken));
        var data = document.RootElement.GetProperty("data");

        Assert.Multiple(() =>
        {
            Assert.That(ok.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(data.GetProperty("healthStatus").GetString(), Is.EqualTo("ONLINE"));
            Assert.That(data.GetProperty("providerStatus").GetString(), Is.EqualTo("ACTIVE"));
            Assert.That(data.GetProperty("latencyMs").GetInt64(), Is.EqualTo(12));
            Assert.That(missing.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(student.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        });
    }

    [Test]
    public async Task ProviderEndpointsRequireAnAdminActor()
    {
        using var noActor = await client.GetAsync(ApiRoutes.Ai.ProvidersServicePath(), TestContext.CurrentContext.CancellationToken);
        using var student = await SendAsync(HttpMethod.Get, ApiRoutes.Ai.ProvidersServicePath(), actorType: ActorHeaderTypes.Student);
        var forbidden = await student.Content.ReadFromJsonAsync<ApiErrorResponse>(TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(noActor.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(student.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            Assert.That(forbidden?.Error.Code, Is.EqualTo(ApiErrorCodes.Forbidden));
        });
    }

    [Test]
    public async Task CreateProviderWithInvalidBodyReturnsValidationDetailsAndPersistsNothing()
    {
        using var response = await SendAsync(HttpMethod.Post, ApiRoutes.Ai.ProvidersServicePath(), new { name = "x", providerType = "NOPE" });
        var envelope = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(envelope?.Error.Code, Is.EqualTo(ApiErrorCodes.ValidationFailed));
            Assert.That(envelope?.Error.Details?.Select(detail => detail.Field), Is.SupersetOf(RequiredInvalidFields));
            Assert.That(store.Providers, Is.Empty);
        });
    }

    [Test]
    public async Task CreateProviderWithDuplicateNameReturnsConflict()
    {
        store.AddProvider("OpenAI main");

        using var response = await SendAsync(HttpMethod.Post, ApiRoutes.Ai.ProvidersServicePath(), new { name = "OpenAI main", providerType = "OPENAI", apiKey = SecretKey });
        var envelope = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(envelope?.Error.Code, Is.EqualTo(AiErrorCodes.ProviderNameConflict));
        });
    }

    [Test]
    public async Task GetProvidersReturnsOffsetPaginationAndNoSecrets()
    {
        store.AddProvider("First");
        store.AddProvider("Second");

        using var response = await SendAsync(HttpMethod.Get, ApiRoutes.Ai.ProvidersServicePath() + "?page=1&pageSize=10");
        var body = await response.Content.ReadAsStringAsync(TestContext.CurrentContext.CancellationToken);
        using var document = JsonDocument.Parse(body);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Headers.CacheControl?.NoStore, Is.True);
            Assert.That(document.RootElement.GetProperty("data").GetArrayLength(), Is.EqualTo(2));
            Assert.That(document.RootElement.GetProperty("meta").GetProperty("pagination").GetProperty("totalItems").GetInt64(), Is.EqualTo(2));
            Assert.That(body, Does.Not.Contain("enc:"));
        });
    }

    [Test]
    public async Task GetProviderWithMalformedOrUnknownIdReturnsValidationOrNotFound()
    {
        using var malformed = await SendAsync(HttpMethod.Get, "/api/ai/providers/not-a-uuid");
        using var unknown = await SendAsync(HttpMethod.Get, ApiRoutes.Ai.ProviderByIdServicePath(Guid.NewGuid()));

        Assert.Multiple(() =>
        {
            Assert.That(malformed.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(unknown.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        });
    }

    [Test]
    public async Task UpdateProviderWithoutApiKeyKeepsStoredKey()
    {
        var provider = store.AddProvider();

        using var response = await SendAsync(HttpMethod.Put, ApiRoutes.Ai.ProviderByIdServicePath(provider.Id), new { name = "Renamed", providerType = "OPENAI", status = "INACTIVE" });

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(store.Providers[provider.Id].Record.Name, Is.EqualTo("Renamed"));
            Assert.That(store.Providers[provider.Id].ApiKeyEncrypted, Is.EqualTo("enc:sk-original-abcd"));
        });
    }

    [Test]
    public async Task DeleteProviderWithModelsReturnsConflictThenNoContentAfterModelRemoved()
    {
        var provider = store.AddProvider();
        var model = store.AddModel(provider);

        using var blocked = await SendAsync(HttpMethod.Delete, ApiRoutes.Ai.ProviderByIdServicePath(provider.Id));
        using var modelDeleted = await SendAsync(HttpMethod.Delete, ApiRoutes.Ai.ModelByIdServicePath(model.Id));
        using var deleted = await SendAsync(HttpMethod.Delete, ApiRoutes.Ai.ProviderByIdServicePath(provider.Id));

        Assert.Multiple(() =>
        {
            Assert.That(blocked.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(modelDeleted.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(deleted.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(store.Providers, Is.Empty);
        });
    }

    [Test]
    public async Task ModelLifecycleCreateListUpdateAndDetail()
    {
        var provider = store.AddProvider();

        using var created = await SendAsync(HttpMethod.Post, ApiRoutes.Ai.ModelsServicePath(), new { providerId = provider.Id, modelName = "gpt-4o" });
        using var createdDocument = JsonDocument.Parse(await created.Content.ReadAsStringAsync(TestContext.CurrentContext.CancellationToken));
        var modelId = createdDocument.RootElement.GetProperty("data").GetProperty("id").GetGuid();
        using var updated = await SendAsync(HttpMethod.Put, ApiRoutes.Ai.ModelByIdServicePath(modelId), new { modelName = "gpt-4o", displayName = "GPT-4o", status = "ACTIVE" });
        using var list = await SendAsync(HttpMethod.Get, $"{ApiRoutes.Ai.ModelsServicePath()}?providerId={provider.Id}");
        using var detail = await SendAsync(HttpMethod.Get, ApiRoutes.Ai.ModelByIdServicePath(modelId));
        using var detailDocument = JsonDocument.Parse(await detail.Content.ReadAsStringAsync(TestContext.CurrentContext.CancellationToken));

        Assert.Multiple(() =>
        {
            Assert.That(created.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(created.Headers.Location?.OriginalString, Is.EqualTo($"/ai/api/ai/models/{modelId:D}"));
            Assert.That(updated.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(list.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(detailDocument.RootElement.GetProperty("data").GetProperty("displayName").GetString(), Is.EqualTo("GPT-4o"));
            Assert.That(detailDocument.RootElement.GetProperty("data").GetProperty("providerName").GetString(), Is.EqualTo(provider.Name));
        });
    }

    [Test]
    public async Task CreateModelForUnknownProviderReturnsNotFound()
    {
        using var response = await SendAsync(HttpMethod.Post, ApiRoutes.Ai.ModelsServicePath(), new { providerId = Guid.NewGuid(), modelName = "gpt-4o" });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task BindingLifecycleUpsertCreatedThenReplacedThenBlockedModelDeleteAndRemoval()
    {
        var provider = store.AddProvider();
        var first = store.AddModel(provider, "gpt-4o");
        var second = store.AddModel(provider, "gpt-4o-mini");
        var path = ApiRoutes.Ai.ServiceBindingByServiceNameServicePath("course-service");

        using var created = await SendAsync(HttpMethod.Put, path, new { modelId = first.Id });
        using var replaced = await SendAsync(HttpMethod.Put, path, new { modelId = second.Id });
        using var blocked = await SendAsync(HttpMethod.Delete, ApiRoutes.Ai.ModelByIdServicePath(second.Id));
        using var detail = await SendAsync(HttpMethod.Get, path);
        using var list = await SendAsync(HttpMethod.Get, ApiRoutes.Ai.ServiceBindingsServicePath());
        using var listDocument = JsonDocument.Parse(await list.Content.ReadAsStringAsync(TestContext.CurrentContext.CancellationToken));
        using var removed = await SendAsync(HttpMethod.Delete, path);
        using var missing = await SendAsync(HttpMethod.Get, path);

        Assert.Multiple(() =>
        {
            Assert.That(created.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(replaced.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(blocked.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(detail.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(listDocument.RootElement.GetProperty("data").GetArrayLength(), Is.EqualTo(1));
            Assert.That(removed.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(missing.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        });
    }

    [Test]
    public async Task UpsertBindingRejectsUnsupportedServiceAndInactiveModel()
    {
        var provider = store.AddProvider();
        var active = store.AddModel(provider, "gpt-4o");
        var inactive = store.AddModel(provider, "old", status: "INACTIVE");

        using var unsupported = await SendAsync(HttpMethod.Put, "/api/ai/service-bindings/ai-service", new { modelId = active.Id });
        using var inactiveResponse = await SendAsync(HttpMethod.Put, "/api/ai/service-bindings/course-service", new { modelId = inactive.Id });
        var inactiveEnvelope = await inactiveResponse.Content.ReadFromJsonAsync<ApiErrorResponse>(TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(unsupported.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(inactiveResponse.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(inactiveEnvelope?.Error.Code, Is.EqualTo(AiErrorCodes.ReferenceInactive));
            Assert.That(store.Bindings, Is.Empty);
        });
    }

    [Test]
    public async Task GetOptionsListsProviderTypesWithDefaultBaseUrlsAndBindableServices()
    {
        using var response = await SendAsync(HttpMethod.Get, ApiRoutes.Ai.OptionsServicePath());
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.CurrentContext.CancellationToken));
        var data = document.RootElement.GetProperty("data");
        var types = data.GetProperty("providerTypes").EnumerateArray().Select(item => item.GetProperty("providerType").GetString()).ToArray();
        var services = data.GetProperty("bindableServices").EnumerateArray().Select(item => item.GetString()).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(types, Is.EquivalentTo(ExpectedProviderTypes));
            Assert.That(services, Does.Contain(ServiceNames.Course));
            Assert.That(services, Does.Not.Contain(ServiceNames.Ai));
        });
    }

    private Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object? body = null, string actorType = ActorHeaderTypes.Admin)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Add(ApiHeaderNames.ActorType, actorType);
        request.Headers.Add(ApiHeaderNames.ActorId, AdminId.ToString());
        if (body is not null) request.Content = JsonContent.Create(body);
        return client.SendAsync(request, TestContext.CurrentContext.CancellationToken);
    }
}
