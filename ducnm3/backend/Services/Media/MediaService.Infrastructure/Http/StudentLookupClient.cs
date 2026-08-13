using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Contracts.Api;
using MediaService.Application;
using MediaService.Application.Actors;

namespace MediaService.Infrastructure.Http;

public sealed class StudentLookupClient(HttpClient httpClient) : IStudentLookup
{
    public async Task<StudentLookupResult?> GetByIdAsync(
        Guid studentId,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.GetAsync(
                $"api/students/{studentId:D}",
                cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                throw DependencyUnavailable();
            }

            var envelope = await response.Content.ReadFromJsonAsync<
                ApiResponse<StudentLookupResult>>(
                cancellationToken: cancellationToken);
            return envelope?.Data ?? throw DependencyUnavailable();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (MediaApplicationException)
        {
            throw;
        }
        catch (Exception)
        {
            throw DependencyUnavailable();
        }
    }

    private static MediaApplicationException DependencyUnavailable() =>
        new(
            MediaErrorCodes.StudentServiceUnavailable,
            "Student Service is temporarily unavailable.",
            503);
}
