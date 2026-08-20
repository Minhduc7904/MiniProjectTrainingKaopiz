using BuildingBlocks.Contracts.Admins;
using BuildingBlocks.Contracts.Api;

namespace AdminService.Application.Features.Admins.GetById;

public sealed class GetAdminByIdHandler(IAdminRepository repository)
{
    public async Task<AdminQueryResponse> HandleAsync(Guid adminId, CancellationToken cancellationToken)
    {
        if (adminId == Guid.Empty)
        {
            throw new AdminApplicationException(ApiErrorCodes.ValidationFailed, ApiErrorMessages.ValidationFailed, 400);
        }

        return await repository.GetByIdAsync(adminId, cancellationToken)
            ?? throw new AdminApplicationException(AdminErrorCodes.NotFound, AdminErrorMessages.NotFound, 404);
    }
}

public static class AdminErrorCodes
{
    public const string NotFound = "ADMIN_NOT_FOUND";
}

public static class AdminErrorMessages
{
    public const string NotFound = "Admin was not found.";
}

public sealed class AdminApplicationException(string code, string message, int statusCode)
    : ApiException(code, message, statusCode);
