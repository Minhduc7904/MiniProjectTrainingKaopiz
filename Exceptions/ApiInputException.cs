using Dtos.ExceptionDto;

namespace Exceptions;

// Sử dụng cho các trường hợp param đầu vào không hợp lệ mà không validate được
public class ApiInputException : Exception
{
    public ApiInputException(string errorCode, DataFailDto errorDetail)
    {
        ErrorDetail = errorDetail;
        ErrorCode = errorCode;
    }

    public DataFailDto? ErrorDetail { get; set; }
    public string ErrorCode { get; set; }
}
