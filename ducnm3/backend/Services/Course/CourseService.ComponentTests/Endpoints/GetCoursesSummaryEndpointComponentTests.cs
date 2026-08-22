using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Extensions;
using CourseService.Api.Endpoints.Courses.GetSummary;
using CourseService.Application;
using CourseService.Application.Repositories;
using CourseService.Application.UseCases.Courses.GetSummary;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace CourseService.ComponentTests.Endpoints;

public sealed class GetCoursesSummaryEndpointComponentTests
{
    [Test]
    public async Task Get_AdminActor_ReturnsSummaryEnvelopeAndNoStore()
    {
        await using var fixture = await Fixture.CreateAsync();
        using var request = new HttpRequestMessage(HttpMethod.Get, ApiRoutes.Courses.SummaryServicePath());
        request.Headers.Add(ApiHeaderNames.ActorType, ActorHeaderTypes.Admin);
        request.Headers.Add(ApiHeaderNames.ActorId, "11111111-1111-1111-1111-111111111111");

        using var response = await fixture.Client.SendAsync(request);
        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<CoursesSummaryResult>>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Headers.CacheControl?.NoStore, Is.True);
            Assert.That(envelope?.Data.TotalCourses, Is.EqualTo(12));
            Assert.That(envelope?.Data.TotalLessons, Is.EqualTo(37));
        });
    }

    private sealed class Repository : ICourseSummaryRepository
    {
        public Task<(long TotalCourses, long TotalLessons)> CountAsync(CancellationToken cancellationToken) =>
            Task.FromResult((12L, 37L));
    }

    private sealed class Fixture(WebApplication app, HttpClient client) : IAsyncDisposable
    {
        public HttpClient Client { get; } = client;

        public static async Task<Fixture> CreateAsync()
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseTestServer();
            builder.Services.AddCourseApplication();
            builder.Services.AddSingleton<ICourseSummaryRepository, Repository>();
            var app = builder.Build();
            app.UseSharedApiMiddleware();
            app.MapGetCoursesSummary();
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
