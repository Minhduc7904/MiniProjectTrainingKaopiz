using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Constant;

namespace Entities;

public class User : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column(TypeName = AttributeDatabase.DEFAULT_TEXT)]
    public string Username { get; set; }

    [Column(TypeName = AttributeDatabase.DEFAULT_TEXT)]
    public string Password { get; set; }

    [Column(TypeName = AttributeDatabase.DEFAULT_TEXT)]
    public UserRole Role { get; set; }

    public ICollection<Order> Orders { get; set; }

    public ICollection<ShipmentLog>? ShipmentLogs { get; set; }
}
