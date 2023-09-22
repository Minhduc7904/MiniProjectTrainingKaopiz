using Dtos.AccountDto;

namespace Services.AccountService;

public interface IUserService
{
    Task<LoginResponseDto> Login();
    Task<int> Signup();
}
