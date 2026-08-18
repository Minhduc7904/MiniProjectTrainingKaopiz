// File: backend/Services/Media/MediaService.Infrastructure/Persistence/Context/MediaDbContext.cs
// Mục đích: Cấu hình EF Core DbContext và mapping database cho service.

﻿using System;
using System.Collections.Generic;
using MediaService.Infrastructure.Persistence.Scaffolded;
using Microsoft.EntityFrameworkCore;

namespace MediaService.Infrastructure.Persistence.Context;

public partial class MediaDbContext : DbContext
{
    public MediaDbContext(DbContextOptions<MediaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<MediaDerivationJob> MediaDerivationJobs { get; set; }

    public virtual DbSet<MediaObject> MediaObjects { get; set; }

    public virtual DbSet<MediaUsage> MediaUsages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<MediaDerivationJob>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("media_derivation_jobs");

            entity.HasIndex(e => new { e.Status, e.UpdatedAt }, "ix_media_derivation_jobs_status_updated_at");

            entity.HasIndex(e => e.DerivativeMediaId, "uq_media_derivation_jobs_derivative").IsUnique();

            entity.HasIndex(e => new { e.SourceMediaId, e.DerivationType }, "uq_media_derivation_jobs_source_type").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("UUID định danh operation tạo media dẫn xuất")
                .HasColumnName("id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.AttemptCount)
                .HasComment("Số lần worker đã bắt đầu xử lý")
                .HasColumnName("attempt_count");
            entity.Property(e => e.CompletedAt)
                .HasMaxLength(6)
                .HasComment("Thời điểm job đạt trạng thái terminal, UTC")
                .HasColumnName("completed_at");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .HasComment("Thời điểm job được tạo, UTC")
                .HasColumnName("created_at");
            entity.Property(e => e.DerivationType)
                .HasMaxLength(32)
                .HasComment("Loại dẫn xuất; hiện chỉ hỗ trợ THUMBNAIL")
                .HasColumnName("derivation_type")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.DerivativeMediaId)
                .HasComment("Media WebP dẫn xuất được cấp trước")
                .HasColumnName("derivative_media_id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.LastError)
                .HasMaxLength(500)
                .HasComment("Lỗi an toàn của lần xử lý cuối; không trả chi tiết nội bộ")
                .HasColumnName("last_error");
            entity.Property(e => e.SourceMediaId)
                .HasComment("Media gốc cần tạo thumbnail")
                .HasColumnName("source_media_id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.StartedAt)
                .HasMaxLength(6)
                .HasComment("Thời điểm lần xử lý gần nhất bắt đầu, UTC")
                .HasColumnName("started_at");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'QUEUED'")
                .HasComment("QUEUED | PROCESSING | READY | FAILED")
                .HasColumnName("status")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.UpdatedAt)
                .HasMaxLength(6)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .HasComment("Thời điểm job cập nhật gần nhất, UTC")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.DerivativeMedia).WithOne(p => p.MediaDerivationJobDerivativeMedia)
                .HasForeignKey<MediaDerivationJob>(d => d.DerivativeMediaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_media_derivation_jobs_derivative");

            entity.HasOne(d => d.SourceMedia).WithMany(p => p.MediaDerivationJobSourceMedia)
                .HasForeignKey(d => d.SourceMediaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_media_derivation_jobs_source");
        });

        modelBuilder.Entity<MediaObject>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("media_objects");

            entity.HasIndex(e => new { e.SourceMediaId, e.DerivationType }, "ix_media_objects_source_derivation");

            entity.HasIndex(e => new { e.IsDraft, e.Status, e.DeletedAt, e.DraftedAt }, "ix_media_objects_draft_cleanup");

            entity.HasIndex(e => new { e.Status, e.CreatedAt }, "ix_media_objects_status_created_at");

            entity.HasIndex(e => new { e.UploadedBy, e.CreatedAt }, "ix_media_objects_uploaded_by_created_at").IsDescending(false, true);

            entity.HasIndex(e => new { e.Bucket, e.ObjectKey }, "uq_media_objects_bucket_object_key").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("UUID định danh media")
                .HasColumnName("id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.Bucket)
                .HasMaxLength(63)
                .HasComment("Tên bucket MinIO chứa object")
                .HasColumnName("bucket")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.ChecksumSha256)
                .HasMaxLength(64)
                .IsFixedLength()
                .HasComment("Hash SHA-256 kiểm tra toàn vẹn; null khi upload chưa READY")
                .HasColumnName("checksum_sha256")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.CompletedAt)
                .HasMaxLength(6)
                .HasComment("Thời điểm upload chuyển READY, UTC")
                .HasColumnName("completed_at");
            entity.Property(e => e.ContentType)
                .HasMaxLength(255)
                .HasComment("MIME type đã xác thực")
                .HasColumnName("content_type")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .HasComment("Thời điểm tạo media record, UTC")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasMaxLength(6)
                .HasComment("Soft-delete timestamp; null khi media còn hoạt động")
                .HasColumnName("deleted_at");
            entity.Property(e => e.DerivationType)
                .HasMaxLength(32)
                .HasComment("THUMBNAIL với object dẫn xuất; null với file upload gốc")
                .HasColumnName("derivation_type")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.DraftedAt)
                .HasMaxLength(6)
                .HasComment("Thời điểm media bắt đầu ở trạng thái draft, UTC")
                .HasColumnName("drafted_at");
            entity.Property(e => e.FailureReason)
                .HasMaxLength(500)
                .HasComment("Lỗi an toàn nội bộ khi upload FAILED; không trả cho client")
                .HasColumnName("failure_reason");
            entity.Property(e => e.MediaType)
                .HasMaxLength(20)
                .HasComment("IMAGE | VIDEO | DOCUMENT | AUDIO | OTHER")
                .HasColumnName("media_type")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.IsDraft)
                .HasDefaultValueSql("'1'")
                .HasComment("Media chưa được gắn với usage active")
                .HasColumnName("is_draft");
            entity.Property(e => e.ObjectKey)
                .HasMaxLength(1024)
                .HasComment("Khóa object duy nhất trong bucket; không trả trực tiếp cho client")
                .HasColumnName("object_key")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.OriginalFileName)
                .HasMaxLength(255)
                .HasComment("Tên file do người dùng upload, chỉ để hiển thị")
                .HasColumnName("original_file_name");
            entity.Property(e => e.SizeBytes)
                .HasComment("Kích thước object theo byte")
                .HasColumnName("size_bytes");
            entity.Property(e => e.SourceMediaId)
                .HasComment("Media gốc của object dẫn xuất; null với file upload gốc")
                .HasColumnName("source_media_id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'PENDING'")
                .HasComment("PENDING | READY | FAILED")
                .HasColumnName("status")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.UpdatedAt)
                .HasMaxLength(6)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .HasComment("Thời điểm media record cập nhật gần nhất, UTC")
                .HasColumnName("updated_at");
            entity.Property(e => e.UploadedBy)
                .HasComment("UUID user/admin upload media; logical reference")
                .HasColumnName("uploaded_by")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.UploadedByType)
                .HasMaxLength(32)
                .HasDefaultValueSql("'STUDENT'")
                .HasComment("Actor type thực hiện upload; được Application validate")
                .HasColumnName("uploaded_by_type")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");

            entity.HasOne(d => d.SourceMedia).WithMany(p => p.InverseSourceMedia)
                .HasForeignKey(d => d.SourceMediaId)
                .HasConstraintName("fk_media_objects_source_media_id");
        });

        modelBuilder.Entity<MediaUsage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("media_usages");

            entity.HasIndex(e => new { e.OwnerService, e.OwnerType, e.OwnerId, e.DisplayOrder }, "ix_media_usages_owner_display_order");

            entity.HasIndex(e => e.ActiveCourseThumbnailOwnerId, "uq_media_usages_active_course_thumbnail").IsUnique();

            entity.HasIndex(e => e.ActiveMediaThumbnailOwnerId, "uq_media_usages_active_media_thumbnail").IsUnique();

            entity.HasIndex(e => new { e.MediaId, e.OwnerService, e.OwnerType, e.OwnerId, e.UsageType, e.ActiveReferenceGuard }, "uq_media_usages_active_reference").IsUnique();

            entity.HasIndex(e => e.ActiveStudentAvatarOwnerId, "uq_media_usages_active_student_avatar").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("UUID định danh liên kết usage")
                .HasColumnName("id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.ActiveCourseThumbnailOwnerId)
                .HasComputedColumnSql("case when ((`owner_type` = _ascii'COURSE_THUMBNAIL') and (`deleted_at` is null)) then `owner_id` else NULL end", true)
                .HasComment("Owner Course có thumbnail còn hiệu lực; dùng để đảm bảo tối đa một thumbnail")
                .HasColumnName("active_course_thumbnail_owner_id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.ActiveMediaThumbnailOwnerId)
                .HasComputedColumnSql("case when ((`owner_service` = _utf8mb4'MEDIA') and (`owner_type` = _utf8mb4'MEDIA_THUMBNAIL') and (`usage_type` = _utf8mb4'THUMBNAIL') and (`deleted_at` is null)) then `owner_id` else NULL end", true)
                .HasComment("Media gốc có thumbnail active; đảm bảo tối đa một thumbnail")
                .HasColumnName("active_media_thumbnail_owner_id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.ActiveReferenceGuard)
                .HasComputedColumnSql("case when (`deleted_at` is null) then 1 else NULL end", true)
                .HasComment("Chỉ áp dụng unique reference cho usage active")
                .HasColumnName("active_reference_guard");
            entity.Property(e => e.ActiveStudentAvatarOwnerId)
                .HasComputedColumnSql("case when ((`owner_service` = _ascii'STUDENT') and (`owner_type` = _ascii'STUDENT_AVATAR') and (`usage_type` = _ascii'AVATAR') and (`deleted_at` is null)) then `owner_id` else NULL end", true)
                .HasComment("Student có avatar active; đảm bảo tối đa một avatar")
                .HasColumnName("active_student_avatar_owner_id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .HasComment("Thời điểm tạo liên kết, UTC")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasComment("UUID user/admin tạo liên kết; logical reference")
                .HasColumnName("created_by")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.CreatedByType)
                .HasMaxLength(32)
                .HasDefaultValueSql("'STUDENT'")
                .HasComment("Actor type tạo usage; được Application validate")
                .HasColumnName("created_by_type")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.DeletedAt)
                .HasMaxLength(6)
                .HasComment("Soft-delete timestamp; null khi usage còn hiệu lực")
                .HasColumnName("deleted_at");
            entity.Property(e => e.DisplayOrder)
                .HasComment("Thứ tự render media trong cùng một owner")
                .HasColumnName("display_order");
            entity.Property(e => e.MediaId)
                .HasComment("UUID media_objects.id trong Media Service database")
                .HasColumnName("media_id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.OwnerId)
                .HasComment("UUID owner ở owner_service; logical reference")
                .HasColumnName("owner_id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.OwnerService)
                .HasMaxLength(20)
                .HasComment("COURSE | NOTIFICATION")
                .HasColumnName("owner_service")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.OwnerType)
                .HasMaxLength(30)
                .HasComment("COURSE_THUMBNAIL | COURSE_DESCRIPTION | LESSON_CONTENT | NOTIFICATION_BODY")
                .HasColumnName("owner_type")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.UsageType)
                .HasMaxLength(20)
                .HasComment("THUMBNAIL | EMBED | ATTACHMENT")
                .HasColumnName("usage_type")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");

            entity.HasOne(d => d.Media).WithMany(p => p.MediaUsages)
                .HasForeignKey(d => d.MediaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_media_usages_media_id");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
