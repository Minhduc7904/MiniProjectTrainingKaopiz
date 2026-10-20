// File: backend/Services/Course/CourseService.Infrastructure/Persistence/MySqlExceptionExtensions.cs
// Mục đích: Nhận diện lỗi unique của MySQL để repository xử lý race condition thay vì trả 500.

using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace CourseService.Infrastructure.Persistence;

internal static class MySqlExceptionExtensions
{
    public static bool IsDuplicateKey(this DbUpdateException exception) =>
        exception.InnerException is MySqlException { ErrorCode: MySqlErrorCode.DuplicateKeyEntry };
}
