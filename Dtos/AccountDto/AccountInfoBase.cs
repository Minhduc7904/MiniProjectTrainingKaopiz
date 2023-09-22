using System.ComponentModel.DataAnnotations;

namespace Dtos.AccountDto;

public class AccountInfoBase
{
    [MaxLength(255)] [Required] public string FullName { get; set; }

    [Required] [MaxLength(12)] public string Phone { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; }

    [Required] [MaxLength(255)] public string Address { get; set; }
}
