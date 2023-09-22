using Constant;

namespace Dtos.AccountDto;

public class LoginResponseDto
{
    public string Token { get; set; }
    public int Id { get; set; }
    public string FullName { get; set; }
    public UserRole Role { get; set; }
}
