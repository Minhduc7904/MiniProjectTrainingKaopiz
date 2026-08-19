using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SchedulerService.Infrastructure.Persistence.Scaffolded;

namespace SchedulerService.Infrastructure.Persistence;

public partial class SchedulerDbContext : DbContext
{
    public SchedulerDbContext(DbContextOptions<SchedulerDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BackgroundJob> BackgroundJobs { get; set; }

    public virtual DbSet<BackgroundJobRun> BackgroundJobRuns { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<BackgroundJob>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("background_jobs");

            entity.HasIndex(e => new { e.Status, e.NextRunAt }, "ix_background_jobs_status_next_run_at");

            entity.HasIndex(e => new { e.TargetService, e.JobType }, "ix_background_jobs_target_service_job_type");

            entity.HasIndex(e => e.JobKey, "uq_background_jobs_job_key").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("UUID định danh cấu hình background job")
                .HasColumnName("id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.AllowConcurrent)
                .HasComment("Cho phép nhiều run đồng thời của cùng job hay không")
                .HasColumnName("allow_concurrent");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .HasComment("Thời điểm tạo job, UTC")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasComment("UUID actor tạo job; null với system-defined job")
                .HasColumnName("created_by")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.CronExpression)
                .HasMaxLength(120)
                .HasComment("Biểu thức CRON theo UTC; null với MANUAL")
                .HasColumnName("cron_expression")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.JobKey)
                .HasMaxLength(100)
                .HasComment("Khóa ổn định, duy nhất dùng để đăng ký và tra cứu job")
                .HasColumnName("job_key")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.JobType)
                .HasMaxLength(100)
                .HasComment("Loại handler tương lai, ví dụ NOTIFICATION_BATCH_DISPATCH hoặc MEDIA_UNUSED_CLEANUP")
                .HasColumnName("job_type")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.MaxRetryCount)
                .HasComment("Số lần retry tối đa dành cho execution phase tương lai")
                .HasColumnName("max_retry_count");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasComment("Tên job hiển thị cho vận hành")
                .HasColumnName("name");
            entity.Property(e => e.NextRunAt)
                .HasMaxLength(6)
                .HasComment("Thời điểm UTC chạy CRON kế tiếp; null khi chưa tính hoặc là MANUAL")
                .HasColumnName("next_run_at");
            entity.Property(e => e.PayloadJson)
                .HasComment("Cấu hình đầu vào chung; không lưu recipient list hoặc dữ liệu domain lớn")
                .HasColumnType("json")
                .HasColumnName("payload_json");
            entity.Property(e => e.ScheduleType)
                .HasMaxLength(20)
                .HasComment("MANUAL | CRON")
                .HasColumnName("schedule_type")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'ACTIVE'")
                .HasComment("ACTIVE | PAUSED | DISABLED")
                .HasColumnName("status")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.TargetService)
                .HasMaxLength(100)
                .HasComment("Service sở hữu nghiệp vụ sẽ được gọi trong phase execution tương lai")
                .HasColumnName("target_service")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.TimeoutSeconds)
                .HasDefaultValueSql("'300'")
                .HasComment("Thời gian chạy tối đa trước khi đánh dấu timeout")
                .HasColumnName("timeout_seconds");
            entity.Property(e => e.UpdatedAt)
                .HasMaxLength(6)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .HasComment("Thời điểm cập nhật job gần nhất, UTC")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<BackgroundJobRun>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("background_job_runs");

            entity.HasIndex(e => new { e.BackgroundJobId, e.CreatedAt }, "ix_background_job_runs_job_created_at").IsDescending(false, true);

            entity.HasIndex(e => new { e.Status, e.ScheduledAt }, "ix_background_job_runs_status_scheduled_at");

            entity.HasIndex(e => new { e.BackgroundJobId, e.IdempotencyKey }, "uq_background_job_runs_job_idempotency").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("UUID định danh một lần thực thi job")
                .HasColumnName("id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.AttemptNumber)
                .HasDefaultValueSql("'1'")
                .HasComment("Lần thử hiện tại, bắt đầu từ 1")
                .HasColumnName("attempt_number");
            entity.Property(e => e.BackgroundJobId)
                .HasComment("UUID background_jobs.id trong Scheduler database")
                .HasColumnName("background_job_id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.CorrelationId)
                .HasMaxLength(128)
                .HasComment("Correlation ID dùng nối log giữa Scheduler và target service")
                .HasColumnName("correlation_id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .HasComment("Thời điểm tạo run, UTC")
                .HasColumnName("created_at");
            entity.Property(e => e.ErrorCode)
                .HasMaxLength(100)
                .HasComment("Mã lỗi ổn định cuối cùng; null khi chưa lỗi")
                .HasColumnName("error_code")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.ErrorMessage)
                .HasComment("Thông tin lỗi an toàn cho vận hành; không chứa credential")
                .HasColumnType("text")
                .HasColumnName("error_message");
            entity.Property(e => e.FinishedAt)
                .HasMaxLength(6)
                .HasComment("Thời điểm worker kết thúc xử lý, UTC")
                .HasColumnName("finished_at");
            entity.Property(e => e.IdempotencyKey)
                .HasMaxLength(128)
                .HasComment("Khóa chống tạo trùng run cho cùng job")
                .HasColumnName("idempotency_key")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.OutputJson)
                .HasComment("Kết quả tóm tắt của run; không dùng thay domain database")
                .HasColumnType("json")
                .HasColumnName("output_json");
            entity.Property(e => e.PayloadSnapshotJson)
                .HasComment("Snapshot payload tại thời điểm tạo run để phục vụ audit")
                .HasColumnType("json")
                .HasColumnName("payload_snapshot_json");
            entity.Property(e => e.ScheduledAt)
                .HasMaxLength(6)
                .HasComment("Thời điểm UTC run được lên lịch hoặc manual trigger")
                .HasColumnName("scheduled_at");
            entity.Property(e => e.StartedAt)
                .HasMaxLength(6)
                .HasComment("Thời điểm worker bắt đầu xử lý, UTC")
                .HasColumnName("started_at");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'QUEUED'")
                .HasComment("QUEUED | RUNNING | SUCCEEDED | FAILED | CANCELLED | TIMED_OUT | SKIPPED")
                .HasColumnName("status")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.TriggerType)
                .HasMaxLength(20)
                .HasComment("MANUAL | CRON | RETRY")
                .HasColumnName("trigger_type")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.WorkerInstance)
                .HasMaxLength(200)
                .HasComment("Định danh worker instance thực thi run")
                .HasColumnName("worker_instance")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");

            entity.HasOne(d => d.BackgroundJob).WithMany(p => p.BackgroundJobRuns)
                .HasForeignKey(d => d.BackgroundJobId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_background_job_runs_background_job_id");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
