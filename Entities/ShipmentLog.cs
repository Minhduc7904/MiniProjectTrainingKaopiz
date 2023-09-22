using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Constant;

namespace Entities;

public class ShipmentLog : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column(TypeName = AttributeDatabase.INT)]
    public int OrderId { get; set; }

    public Order Order { get; set; }

    [Column(TypeName = AttributeDatabase.INT)]
    public int? ShipperUserId { get; set; }

    public User? ShipperUser { get; set; }

    [Column(TypeName = AttributeDatabase.DEFAULT_TEXT)]
    public string? Note { get; set; }

    [Column(TypeName = AttributeDatabase.BOOL)]
    public bool? IsSuccess { get; set; }
}
