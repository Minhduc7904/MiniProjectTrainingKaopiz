namespace Dtos;

public class PageResponseDto
{
    public long TotalElement { get; set; }
    public int Limit { get; set; }
    public int Offset { get; set; }
}