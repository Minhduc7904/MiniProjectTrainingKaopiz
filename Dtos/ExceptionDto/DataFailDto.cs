namespace Dtos.ExceptionDto;

public class DataFailDto
{
    public string? Field { get; set; }
    public ICollection<string>? Error { get; set; }
}
