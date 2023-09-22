using System.ComponentModel.DataAnnotations.Schema;
using Constant;

namespace Entities;

public abstract class BaseEntity
{
    [Column(TypeName = AttributeDatabase.DATE_TIME)]
    public DateTime? DeletedAt { get; set; }

    [Column(TypeName = AttributeDatabase.DATE_TIME)]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = AttributeDatabase.INT)]
    public int CreatedUserId { get; set; }

    [Column(TypeName = AttributeDatabase.DATE_TIME)]
    public DateTime? UpdatedAt { get; set; }

    [Column(TypeName = AttributeDatabase.INT)]
    public int? UpdatedUserId { get; set; }
}
