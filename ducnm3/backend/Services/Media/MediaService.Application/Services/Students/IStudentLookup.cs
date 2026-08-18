// File: backend/Services/Media/MediaService.Application/Services/Students/IStudentLookup.cs
// Mục đích: Định nghĩa port tra cứu học viên để xác thực owner thuộc Student Service trước khi tạo media usage.

using BuildingBlocks.Contracts.Students;

namespace MediaService.Application.Services.Students;

public interface IStudentLookup
{
    Task<StudentQueryResponse?> GetByIdAsync(
        Guid studentId,
        CancellationToken cancellationToken);
}
