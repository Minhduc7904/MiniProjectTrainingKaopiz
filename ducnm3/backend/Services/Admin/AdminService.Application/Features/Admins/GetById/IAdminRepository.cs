using BuildingBlocks.Contracts.Admins;

namespace AdminService.Application.Features.Admins.GetById;

public interface IAdminRepository
{
    Task<AdminQueryResponse?> GetByIdAsync(Guid adminId, CancellationToken cancellationToken);
}
