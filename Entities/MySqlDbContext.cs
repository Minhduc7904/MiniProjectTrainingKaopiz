using Microsoft.EntityFrameworkCore;

namespace Entities;

public class MySqlDbContext : DbContext
{
    public MySqlDbContext(DbContextOptions<MySqlDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; }
    public DbSet<ShipmentLog> ShipmentLogs { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserInfo> UserInfo { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
}
