// File: backend/Services/Media/MediaService.Application/Services/Students/IStudentLookup.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

using BuildingBlocks.Contracts.Students;

namespace MediaService.Application.Services.Students;

public interface IStudentLookup
{
    Task<StudentQueryResponse?> GetByIdAsync(
        Guid studentId,
        CancellationToken cancellationToken);
}
