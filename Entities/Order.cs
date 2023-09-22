using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Constant;

namespace Entities;

public class Order : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column(TypeName = AttributeDatabase.INT)]
    public OrderStatus Status { get; set; }

    [Column(TypeName = AttributeDatabase.INT)]
    public int CustomerUserId { get; set; }

    public User CustomerUser { get; set; }

    [Column(TypeName = AttributeDatabase.DATE_TIME)]
    public DateTime? ReceiveDate { get; set; }

    [Column(TypeName = AttributeDatabase.DEFAULT_TEXT)]
    public string ReceiverName { get; set; }

    [Column(TypeName = AttributeDatabase.DEFAULT_TEXT)]
    public string DeliveryAddress { get; set; }

    [Column(TypeName = AttributeDatabase.DOUBLE)]
    public double ShipFee { get; set; }

    [Column(TypeName = AttributeDatabase.DEFAULT_TEXT)]
    public ShipMethod ShipMethod { get; set; }

    [Column(TypeName = AttributeDatabase.DEFAULT_TEXT)]
    public string Phone { get; set; }

    [Column(TypeName = AttributeDatabase.DEFAULT_TEXT)]
    public string? Email { get; set; }

    public ICollection<ShipmentLog>? ShipmentLogs { get; set; }
}
