using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StudentService.Api.Endpoints.Students.GetSummary;
using StudentService.Application;
using StudentService.Application.Repositories;
using StudentService.Application.UseCases.Students.GetSummary;

namespace StudentService.ComponentTests.Endpoints;

public sealed class GetStudentsSummaryEndpointComponentTests
{
    [Test]
    public async Task Get_AdminActor_ReturnsSummaryEnvelopeAndNoStore()
    {
        await using var fixture = await Fixture.CreateAsync();
        using var request = new HttpRequestMessage(HttpMethod.Get, ApiRoutes.Students.SummaryServicePath());
        request.Headers.Add(ApiHeaderNames.ActorType, ActorHeaderTypes.Admin);
        request.Headers.Add(ApiHeaderNames.ActorId, "11111111-1111-1111-1111-111111111111");

        using var response = await fixture.Client.SendAsync(request);
        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<StudentsSummaryResult>>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Headers.CacheControl?.NoStore, Is.True);
            Assert.That(envelope?.Data.TotalStudents, Is.EqualTo(42));
            Assert.That(envelope?.Meta.TraceId, Is.Not.Empty);
        });
    }

    [Test]
    public async Task Get_MissingActor_ReturnsValidationEnvelope()
    {
        await using var fixture = await Fixture.CreateAsync();
        using var response = await fixture.Client.GetAsync(ApiRoutes.Students.SummaryServicePath());
        var envelope = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(envelope?.Error.Code, Is.EqualTo(ApiErrorCodes.ValidationFailed));
    }

    private sealed class Repository : IStudentSummaryRepository
    {
        public Task<long> CountAsync(CancellationToken cancellationToken) => Task.FromResult(42L);
    }

    private sealed class Fixture(WebApplication app, HttpClient client) : IAsyncDisposable
    {
        public HttpClient Client { get; } = client;

        public static async Task<Fixture> CreateAsync()
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseTestServer();
            builder.Services.AddStudentApplication();
            builder.Services.AddSingleton<IStudentSummaryRepository, Repository>();
            var app = builder.Build();
            app.UseSharedApiMiddleware();
            app.MapGetStudentsSummary();
            await app.StartAsync();
            return new Fixture(app, app.GetTestClient());
        }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await app.DisposeAsync();
        }
    }
}
