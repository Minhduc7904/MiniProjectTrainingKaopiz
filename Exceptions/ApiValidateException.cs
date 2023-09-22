using Dtos.ExceptionDto;

namespace Exceptions;

public class ApiValidateException : Exception
{
    public ApiValidateException(string errorCode, List<DataFailDto>? errorDetail)
    {
        ErrorDetail = errorDetail;
        ErrorCode = errorCode;
    }

    public List<DataFailDto>? ErrorDetail { get; set; }
    public string ErrorCode { get; set; }
}
