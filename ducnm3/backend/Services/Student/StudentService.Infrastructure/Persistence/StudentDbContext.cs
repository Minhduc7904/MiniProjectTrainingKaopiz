using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using StudentService.Infrastructure.Persistence.Scaffolded;

namespace StudentService.Infrastructure.Persistence;

public partial class StudentDbContext : DbContext
{
    public StudentDbContext(DbContextOptions<StudentDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Student> Students { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("students");

            entity.HasIndex(e => new { e.Status, e.CreatedAt }, "ix_students_status_created_at").IsDescending(false, true);

            entity.HasIndex(e => e.Email, "uq_students_email").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("UUID định danh Student")
                .HasColumnName("id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .HasComment("Thời điểm tạo Student, UTC")
                .HasColumnName("created_at");
            entity.Property(e => e.DisplayName)
                .HasMaxLength(200)
                .HasComment("Tên hiển thị của Student")
                .HasColumnName("display_name");
            entity.Property(e => e.Email)
                .HasMaxLength(320)
                .HasComment("Email đăng nhập hoặc liên hệ; unique")
                .HasColumnName("email");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasComment("ACTIVE | INACTIVE | BLOCKED")
                .HasColumnName("status")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.UpdatedAt)
                .HasMaxLength(6)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .HasComment("Thời điểm cập nhật Student gần nhất, UTC")
                .HasColumnName("updated_at");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
