using AdminService.Application.Features.Admins.GetById;
using BuildingBlocks.Contracts.Admins;

namespace AdminService.Infrastructure;

public static class ConfigurationAdminRepositoryFactory
{
    public static IAdminRepository Create(IEnumerable<string> ids, string displayName)
    {
        return new ConfigurationAdminRepository(ids.ToArray(), displayName);
    }
}

internal sealed class ConfigurationAdminRepository(string[] ids, string displayName) : IAdminRepository
{
    private readonly HashSet<Guid> adminIds = ids
        .Where(id => Guid.TryParse(id, out _))
        .Select(Guid.Parse)
        .ToHashSet();

    public Task<AdminQueryResponse?> GetByIdAsync(Guid adminId, CancellationToken cancellationToken) =>
        Task.FromResult<AdminQueryResponse?>(adminIds.Contains(adminId)
            ? new AdminQueryResponse(adminId, displayName, "ACTIVE")
            : null);
}
