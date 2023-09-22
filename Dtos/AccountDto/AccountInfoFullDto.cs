using Constant;

namespace Dtos.AccountDto;

public class AccountInfoFullDto
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
    public UserRole Role { get; set; }
    public string Username { get; set; }
}
