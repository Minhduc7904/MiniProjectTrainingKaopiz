using Constant;

namespace Dtos.AccountDto;

public class AccountInfoWithRoleDto : AccountInfoBase
{
    public UserRole Role { get; set; }
}
