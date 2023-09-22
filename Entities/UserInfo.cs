using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Constant;

namespace Entities;

public class UserInfo : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column(TypeName = AttributeDatabase.DEFAULT_TEXT)]
    public string FullName { get; set; }

    [Column(TypeName = AttributeDatabase.DEFAULT_TEXT)]
    public string? Phone { get; set; }

    [Column(TypeName = AttributeDatabase.DEFAULT_TEXT)]
    public string? Email { get; set; }

    [Column(TypeName = AttributeDatabase.INT)]
    public int? UserId { get; set; }

    public User? User { get; set; }
}
