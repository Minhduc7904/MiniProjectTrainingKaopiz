using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Contracts.Admins;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Http;
using MediaService.Application;
using MediaService.Application.Common.Errors;
using MediaService.Application.Services.Admins;

namespace MediaService.Infrastructure.Clients.Admin;

public sealed class AdminLookupClient(HttpClient httpClient) : IAdminLookup
{
    public async Task<AdminQueryResponse?> GetByIdAsync(Guid adminId, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.GetAsync(ApiRoutes.Admins.GetByIdServicePath(adminId), cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound) return null;
            if (!response.IsSuccessStatusCode) throw MediaErrors.AdminServiceUnavailable();
            var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<AdminQueryResponse>>(cancellationToken: cancellationToken);
            return envelope?.Data ?? throw MediaErrors.AdminServiceUnavailable();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (MediaApplicationException) { throw; }
        catch (Exception) { throw MediaErrors.AdminServiceUnavailable(); }
    }
}
