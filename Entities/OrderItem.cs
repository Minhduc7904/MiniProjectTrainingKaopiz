using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Constant;

namespace Entities;

public class OrderItem : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column(TypeName = AttributeDatabase.INT)]
    public int OrderId { get; set; }

    public Order Order { get; set; }

    [Column(TypeName = AttributeDatabase.DEFAULT_TEXT)]
    public string ItemName { get; set; }

    [Column(TypeName = AttributeDatabase.INT)]
    public int Quantity { get; set; }
}
