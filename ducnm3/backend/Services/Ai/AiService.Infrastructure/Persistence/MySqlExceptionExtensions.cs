// File: backend/Services/Ai/AiService.Infrastructure/Persistence/MySqlExceptionExtensions.cs
// Mục đích: Nhận diện lỗi unique/foreign key của MySQL để repository trả lỗi nghiệp vụ thay vì 500 khi có race condition.

using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace AiService.Infrastructure.Persistence;

internal static class MySqlExceptionExtensions
{
    public static bool IsDuplicateKey(this DbUpdateException exception) =>
        exception.InnerException is MySqlException { ErrorCode: MySqlErrorCode.DuplicateKeyEntry };

    public static bool IsForeignKeyViolation(this DbUpdateException exception) =>
        exception.InnerException is MySqlException
        {
            ErrorCode: MySqlErrorCode.RowIsReferenced or MySqlErrorCode.RowIsReferenced2
                or MySqlErrorCode.NoReferencedRow or MySqlErrorCode.NoReferencedRow2
        };
}
