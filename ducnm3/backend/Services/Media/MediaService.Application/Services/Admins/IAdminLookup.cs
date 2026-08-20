using BuildingBlocks.Contracts.Admins;

namespace MediaService.Application.Services.Admins;

public interface IAdminLookup
{
    Task<AdminQueryResponse?> GetByIdAsync(Guid adminId, CancellationToken cancellationToken);
}
