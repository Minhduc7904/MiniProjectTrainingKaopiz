using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Contracts.Students;
using MediaService.Application;
using MediaService.Application.Abstractions.Clients;

namespace MediaService.Infrastructure.Clients.Student;

public sealed class StudentLookupClient(HttpClient httpClient) : IStudentLookup
{
    public async Task<StudentQueryResponse?> GetByIdAsync(
        Guid studentId,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.GetAsync(
                ApiRoutes.Students.GetByIdServicePath(studentId),
                cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                throw MediaErrors.StudentServiceUnavailable();
            }

            var envelope = await response.Content.ReadFromJsonAsync<
                ApiResponse<StudentQueryResponse>>(
                cancellationToken: cancellationToken);
            return envelope?.Data ??
                throw MediaErrors.StudentServiceUnavailable();
        }
        catch (OperationCanceledException) when (
            cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (MediaApplicationException)
        {
            throw;
        }
        catch (Exception)
        {
            throw MediaErrors.StudentServiceUnavailable();
        }
    }
}
