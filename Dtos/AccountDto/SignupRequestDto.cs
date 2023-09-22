using System.ComponentModel.DataAnnotations;

namespace Dtos.AccountDto;

public class SignupRequestDto : AccountInfoBase
{
    [Required]
    [MaxLength(255)]
    [MinLength(4)]
    public string Username { get; set; }

    [MaxLength(255)]
    [MinLength(8)]
    [Required]
    public string Password { get; set; }
}
