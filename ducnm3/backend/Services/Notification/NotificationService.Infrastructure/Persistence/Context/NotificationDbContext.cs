// File: backend/Services/Notification/NotificationService.Infrastructure/Persistence/Context/NotificationDbContext.cs
// Mục đích: Quản lý EF DbSet và mapping scaffolded cho notifications, batches và batch items trong MySQL.

﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using NotificationService.Infrastructure.Persistence.Scaffolded;

namespace NotificationService.Infrastructure.Persistence.Context;

public partial class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<NotificationBatch> NotificationBatches { get; set; }

    public virtual DbSet<NotificationBatchItem> NotificationBatchItems { get; set; }

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

            entity.HasIndex(e => new { e.NotificationBatchId, e.RecipientStudentId }, "uq_notifications_batch_recipient").IsUnique();

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
            entity.Property(e => e.NotificationBatchId)
                .HasComment("UUID notification_batches.id; null với gửi đơn")
                .HasColumnName("notification_batch_id")
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

            entity.HasOne(d => d.NotificationBatch).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.NotificationBatchId)
                .HasConstraintName("fk_notifications_notification_batch_id");
        });

        modelBuilder.Entity<NotificationBatch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("notification_batches");

            entity.HasIndex(e => new { e.Status, e.CreatedAt }, "ix_notification_batches_status_created_at").IsDescending(false, true);

            entity.Property(e => e.Id)
                .HasComment("UUID định danh yêu cầu gửi notification hàng loạt")
                .HasColumnName("id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.BatchSize)
                .HasComment("Số item nghiệp vụ xử lý trên mỗi chunk")
                .HasColumnName("batch_size");
            entity.Property(e => e.BodyMarkdown)
                .HasComment("Nội dung Markdown dùng cho toàn batch; có thể nhúng media")
                .HasColumnType("mediumtext")
                .HasColumnName("body_markdown");
            entity.Property(e => e.CompletedAt)
                .HasMaxLength(6)
                .HasComment("Thời điểm kết thúc batch, UTC")
                .HasColumnName("completed_at");
            entity.Property(e => e.CourseId)
                .HasComment("UUID Course liên quan; null nếu không gửi theo Course")
                .HasColumnName("course_id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .HasComment("Thời điểm tạo batch, UTC")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasComment("UUID admin tạo batch; logical reference")
                .HasColumnName("created_by")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.FailedCount)
                .HasComment("Số recipient thất bại sau retry")
                .HasColumnName("failed_count");
            entity.Property(e => e.ProcessedCount)
                .HasComment("Số recipient đã được xử lý")
                .HasColumnName("processed_count");
            entity.Property(e => e.StartedAt)
                .HasMaxLength(6)
                .HasComment("Thời điểm bắt đầu xử lý batch, UTC")
                .HasColumnName("started_at");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasComment("PENDING | SNAPSHOTTING | SNAPSHOT_READY | PROCESSING | COMPLETED | PARTIAL_FAILED | FAILED")
                .HasColumnName("status")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.SuccessCount)
                .HasComment("Số notification inbox tạo thành công")
                .HasColumnName("success_count");
            entity.Property(e => e.TargetScope)
                .HasMaxLength(20)
                .HasComment("COURSE_ENROLLED | STUDENT_IDS | ALL_STUDENTS")
                .HasColumnName("target_scope")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasComment("Tiêu đề notification dùng cho toàn batch")
                .HasColumnName("title");
            entity.Property(e => e.TotalCount)
                .HasComment("Tổng recipient đã snapshot khi tạo batch")
                .HasColumnName("total_count");
        });

        modelBuilder.Entity<NotificationBatchItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("notification_batch_items");

            entity.HasIndex(e => e.NotificationId, "fk_notification_batch_items_notification_id");

            entity.HasIndex(e => new { e.BatchId, e.Status }, "ix_notification_batch_items_batch_status");

            entity.HasIndex(e => new { e.BatchId, e.Status, e.LeaseExpiresAt, e.Id }, "ix_notification_batch_items_claim");

            entity.HasIndex(e => new { e.BatchId, e.StudentId }, "uq_notification_batch_items_batch_student").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("UUID định danh recipient trong batch")
                .HasColumnName("id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.BatchId)
                .HasComment("UUID notification_batches.id")
                .HasColumnName("batch_id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.ErrorMessage)
                .HasComment("Lỗi cuối cùng; null khi thành công")
                .HasColumnType("text")
                .HasColumnName("error_message");
            entity.Property(e => e.LeaseExpiresAt)
                .HasMaxLength(6)
                .HasComment("Thời điểm UTC claim PROCESSING hết hạn để worker khác có thể nhận lại")
                .HasColumnName("lease_expires_at");
            entity.Property(e => e.LeaseToken)
                .HasComment("UUID token sở hữu claim PROCESSING hiện tại; null khi item chưa được claim hoặc đã hoàn tất")
                .HasColumnName("lease_token")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.NotificationId)
                .HasComment("UUID notifications.id được tạo; null khi chưa thành công")
                .HasColumnName("notification_id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.ProcessedAt)
                .HasMaxLength(6)
                .HasComment("Thời điểm xử lý thành công hoặc thất bại cuối, UTC")
                .HasColumnName("processed_at");
            entity.Property(e => e.RetryCount)
                .HasComment("Số lần retry item nghiệp vụ đã thực hiện")
                .HasColumnName("retry_count");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'PENDING'")
                .HasComment("PENDING | PROCESSING | SUCCESS | RETRY | FAILED")
                .HasColumnName("status")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.StudentId)
                .HasComment("UUID Student nhận notification; logical reference")
                .HasColumnName("student_id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");

            entity.HasOne(d => d.Batch).WithMany(p => p.NotificationBatchItems)
                .HasForeignKey(d => d.BatchId)
                .HasConstraintName("fk_notification_batch_items_batch_id");

            entity.HasOne(d => d.Notification).WithMany(p => p.NotificationBatchItems)
                .HasForeignKey(d => d.NotificationId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_notification_batch_items_notification_id");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
