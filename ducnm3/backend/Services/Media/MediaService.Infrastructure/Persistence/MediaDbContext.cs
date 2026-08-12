using System;
using System.Collections.Generic;
using MediaService.Infrastructure.Persistence.Scaffolded;
using Microsoft.EntityFrameworkCore;

namespace MediaService.Infrastructure.Persistence;

public partial class MediaDbContext : DbContext
{
    public MediaDbContext(DbContextOptions<MediaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<MediaObject> MediaObjects { get; set; }

    public virtual DbSet<MediaUsage> MediaUsages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<MediaObject>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("media_objects");

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
                .HasComment("Hash SHA-256 kiểm tra toàn vẹn")
                .HasColumnName("checksum_sha256")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.ContentType)
                .HasMaxLength(255)
                .HasComment("MIME type đã xác thực")
                .HasColumnName("content_type")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .HasComment("Thời điểm upload hoàn tất, UTC")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasMaxLength(6)
                .HasComment("Soft-delete timestamp; null khi media còn hoạt động")
                .HasColumnName("deleted_at");
            entity.Property(e => e.MediaType)
                .HasMaxLength(20)
                .HasComment("IMAGE | VIDEO | DOCUMENT | AUDIO | OTHER")
                .HasColumnName("media_type")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
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
            entity.Property(e => e.UploadedBy)
                .HasComment("UUID user/admin upload media; logical reference")
                .HasColumnName("uploaded_by")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
        });

        modelBuilder.Entity<MediaUsage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("media_usages");

            entity.HasIndex(e => new { e.OwnerService, e.OwnerType, e.OwnerId, e.DisplayOrder }, "ix_media_usages_owner_display_order");

            entity.HasIndex(e => e.ActiveCourseThumbnailOwnerId, "uq_media_usages_active_course_thumbnail").IsUnique();

            entity.HasIndex(e => new { e.MediaId, e.OwnerService, e.OwnerType, e.OwnerId, e.UsageType }, "uq_media_usages_reference").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("UUID định danh liên kết usage")
                .HasColumnName("id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.ActiveCourseThumbnailOwnerId)
                .HasComputedColumnSql("case when ((`owner_type` = _utf8mb4'COURSE_THUMBNAIL') and (`deleted_at` is null)) then `owner_id` else NULL end", true)
                .HasComment("Owner Course có thumbnail còn hiệu lực; dùng để đảm bảo tối đa một thumbnail")
                .HasColumnName("active_course_thumbnail_owner_id")
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
