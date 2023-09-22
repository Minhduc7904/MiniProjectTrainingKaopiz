using System.ComponentModel.DataAnnotations;

namespace Dtos.AccountDto;

public class LoginRequestDto
{
    [Required]
    [MaxLength(255)]
    [MinLength(4)]
    public string Username { get; set; }

    [Required] [MaxLength(255)] public string Password { get; set; }
}
