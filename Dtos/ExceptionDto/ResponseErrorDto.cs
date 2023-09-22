namespace Dtos.ExceptionDto;

public class ResponseErrorDto<T>
{
    public string ErrorCode { get; set; } = string.Empty;
    public T? ErrorDetail { get; set; }
}
