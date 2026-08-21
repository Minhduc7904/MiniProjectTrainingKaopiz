using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;

namespace BuildingBlocks.Presentation.Tests.Endpoints;

public sealed class ActorPolicyEndpointTests
{
    [Test]
    public async Task GetAdminPolicyWithNormalizedAdminHeaderReturnsActor()
    {
        await using var app = await CreateApplicationAsync();
        using var client = app.GetTestClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/admin");
        request.Headers.Add(ApiHeaderNames.ActorType, " admin ");
        request.Headers.Add(ApiHeaderNames.ActorId, "11111111-1111-1111-1111-111111111111");

        using var response = await client.SendAsync(request);
        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<ActorContext>>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(envelope?.Data.Type, Is.EqualTo(ActorHeaderTypes.Admin));
            Assert.That(envelope?.Data.Id, Is.EqualTo(Guid.Parse("11111111-1111-1111-1111-111111111111")));
        });
    }

    [Test]
    public async Task GetAdminPolicyWithStudentHeaderReturnsForbiddenEnvelope()
    {
        await using var app = await CreateApplicationAsync();
        using var client = app.GetTestClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/admin");
        request.Headers.Add(ApiHeaderNames.ActorType, ActorHeaderTypes.Student);
        request.Headers.Add(ApiHeaderNames.ActorId, Guid.NewGuid().ToString());

        using var response = await client.SendAsync(request);
        var envelope = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            Assert.That(envelope?.Error.Code, Is.EqualTo(ApiErrorCodes.Forbidden));
        });
    }

    [TestCase(null, "11111111-1111-1111-1111-111111111111")]
    [TestCase("UNKNOWN", "11111111-1111-1111-1111-111111111111")]
    [TestCase("ADMIN", "00000000-0000-0000-0000-000000000000")]
    public async Task GetAnyPolicyWithInvalidActorHeaderReturnsValidationEnvelope(
        string? actorType,
        string actorId)
    {
        await using var app = await CreateApplicationAsync();
        using var client = app.GetTestClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/any");
        if (actorType is not null) request.Headers.Add(ApiHeaderNames.ActorType, actorType);
        request.Headers.Add(ApiHeaderNames.ActorId, actorId);

        using var response = await client.SendAsync(request);
        var envelope = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(envelope?.Error.Code, Is.EqualTo(ApiErrorCodes.ValidationFailed));
        });
    }

    private static async Task<WebApplication> CreateApplicationAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        var app = builder.Build();
        app.UseSharedApiMiddleware();
        app.MapGet("/admin", (HttpContext context) => Results.Json(
                ApiResponseFactory.Success(context.GetRequiredActor(), context.TraceIdentifier)))
            .RequireActor(ActorAccess.Admin);
        app.MapGet("/any", () => Results.Ok())
            .RequireActor(ActorAccess.Any);
        await app.StartAsync();
        return app;
    }
}
