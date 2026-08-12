using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using NotificationService.Infrastructure.Persistence.Scaffolded;

namespace NotificationService.Infrastructure.Persistence;

public partial class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<NotificationJob> NotificationJobs { get; set; }

    public virtual DbSet<NotificationJobItem> NotificationJobItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("notifications");

            entity.HasIndex(e => new { e.RecipientStudentId, e.Status, e.CreatedAt }, "ix_notifications_recipient_status_created_at").IsDescending(false, false, true);

            entity.HasIndex(e => new { e.NotificationJobId, e.RecipientStudentId }, "uq_notifications_job_recipient").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("UUID định danh inbox item")
                .HasColumnName("id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.BodyMarkdown)
                .HasComment("Nội dung Markdown; media nhúng dùng URL Media Service")
                .HasColumnType("mediumtext")
                .HasColumnName("body_markdown");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .HasComment("Thời điểm notification xuất hiện trong inbox, UTC")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasComment("UUID admin hoặc system tạo notification; logical reference")
                .HasColumnName("created_by")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.NotificationJobId)
                .HasComment("UUID notification_jobs.id; null với gửi đơn")
                .HasColumnName("notification_job_id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.ReadAt)
                .HasMaxLength(6)
                .HasComment("Thời điểm recipient đánh dấu đã đọc; null khi UNREAD")
                .HasColumnName("read_at");
            entity.Property(e => e.RecipientStudentId)
                .HasComment("UUID Student sở hữu notification; logical reference")
                .HasColumnName("recipient_student_id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.SourceType)
                .HasMaxLength(20)
                .HasComment("SINGLE | BULK")
                .HasColumnName("source_type")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'UNREAD'")
                .HasComment("UNREAD | READ")
                .HasColumnName("status")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasComment("Tiêu đề hiển thị trong inbox")
                .HasColumnName("title");

            entity.HasOne(d => d.NotificationJob).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.NotificationJobId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_notifications_notification_job_id");
        });

        modelBuilder.Entity<NotificationJob>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("notification_jobs");

            entity.HasIndex(e => new { e.Status, e.CreatedAt }, "ix_notification_jobs_status_created_at").IsDescending(false, true);

            entity.Property(e => e.Id)
                .HasComment("UUID định danh batch job")
                .HasColumnName("id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.BatchSize)
                .HasComment("Số item xử lý trên mỗi chunk")
                .HasColumnName("batch_size");
            entity.Property(e => e.BodyMarkdown)
                .HasComment("Nội dung Markdown; có thể nhúng media")
                .HasColumnType("mediumtext")
                .HasColumnName("body_markdown");
            entity.Property(e => e.CompletedAt)
                .HasMaxLength(6)
                .HasComment("Thời điểm job kết thúc; null khi chưa hoàn tất")
                .HasColumnName("completed_at");
            entity.Property(e => e.CourseId)
                .HasComment("UUID Course liên quan; null nếu không gửi theo Course")
                .HasColumnName("course_id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .HasComment("Thời điểm tạo job, UTC")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasComment("UUID admin tạo job; logical reference")
                .HasColumnName("created_by")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.FailedCount)
                .HasComment("Số recipient thất bại sau retry")
                .HasColumnName("failed_count");
            entity.Property(e => e.ProcessedCount)
                .HasComment("Số recipient worker đã xử lý")
                .HasColumnName("processed_count");
            entity.Property(e => e.StartedAt)
                .HasMaxLength(6)
                .HasComment("Thời điểm worker bắt đầu; null khi job chưa chạy")
                .HasColumnName("started_at");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasComment("PENDING | PROCESSING | COMPLETED | PARTIAL_FAILED | FAILED")
                .HasColumnName("status")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.SuccessCount)
                .HasComment("Số notification tạo thành công")
                .HasColumnName("success_count");
            entity.Property(e => e.TargetScope)
                .HasMaxLength(20)
                .HasComment("COURSE_ENROLLED | STUDENT_IDS | ALL_STUDENTS")
                .HasColumnName("target_scope")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasComment("Tiêu đề thông báo")
                .HasColumnName("title");
            entity.Property(e => e.TotalCount)
                .HasComment("Tổng recipient đã snapshot khi tạo job")
                .HasColumnName("total_count");
        });

        modelBuilder.Entity<NotificationJobItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("notification_job_items");

            entity.HasIndex(e => e.NotificationId, "fk_notification_job_items_notification_id");

            entity.HasIndex(e => new { e.JobId, e.Status }, "ix_notification_job_items_job_status");

            entity.HasIndex(e => new { e.JobId, e.StudentId }, "uq_notification_job_items_job_student").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("UUID định danh recipient trong batch")
                .HasColumnName("id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.ErrorMessage)
                .HasComment("Lỗi cuối cùng; null khi thành công")
                .HasColumnType("text")
                .HasColumnName("error_message");
            entity.Property(e => e.JobId)
                .HasComment("UUID notification_jobs.id")
                .HasColumnName("job_id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.NotificationId)
                .HasComment("UUID notifications.id được tạo; null khi chưa thành công")
                .HasColumnName("notification_id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.ProcessedAt)
                .HasMaxLength(6)
                .HasComment("Thời điểm xử lý thành công hoặc thất bại cuối; null khi chưa xử lý")
                .HasColumnName("processed_at");
            entity.Property(e => e.RetryCount)
                .HasComment("Số lần retry đã thực hiện")
                .HasColumnName("retry_count");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'PENDING'")
                .HasComment("PENDING | PROCESSING | SUCCESS | RETRY | FAILED")
                .HasColumnName("status")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.StudentId)
                .HasComment("UUID Student nhận thông báo; logical reference")
                .HasColumnName("student_id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");

            entity.HasOne(d => d.Job).WithMany(p => p.NotificationJobItems)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("fk_notification_job_items_job_id");

            entity.HasOne(d => d.Notification).WithMany(p => p.NotificationJobItems)
                .HasForeignKey(d => d.NotificationId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_notification_job_items_notification_id");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
