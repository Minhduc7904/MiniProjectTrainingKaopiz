using System;
using System.Collections.Generic;
using CourseService.Infrastructure.Persistence.Scaffolded;
using Microsoft.EntityFrameworkCore;

namespace CourseService.Infrastructure.Persistence;

public partial class CourseDbContext : DbContext
{
    public CourseDbContext(DbContextOptions<CourseDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Enrollment> Enrollments { get; set; }

    public virtual DbSet<Lesson> Lessons { get; set; }

    public virtual DbSet<LessonProgress> LessonProgresses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("courses");

            entity.HasIndex(e => new { e.Status, e.CreatedAt }, "ix_courses_status_created_at").IsDescending(false, true);

            entity.Property(e => e.Id)
                .HasComment("UUID định danh Course")
                .HasColumnName("id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .HasComment("Thời điểm tạo Course, UTC")
                .HasColumnName("created_at");
            entity.Property(e => e.DescriptionMarkdown)
                .HasComment("Nội dung mô tả Markdown; có thể nhúng media")
                .HasColumnType("mediumtext")
                .HasColumnName("description_markdown");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasComment("Tên Course hiển thị cho người dùng")
                .HasColumnName("name");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasComment("DRAFT | PUBLISHED | ARCHIVED")
                .HasColumnName("status")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.UpdatedAt)
                .HasMaxLength(6)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .HasComment("Thời điểm cập nhật gần nhất, UTC")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("enrollments");

            entity.HasIndex(e => new { e.StudentId, e.EnrolledAt }, "ix_enrollments_student_id_enrolled_at").IsDescending(false, true);

            entity.HasIndex(e => new { e.CourseId, e.StudentId }, "uq_enrollments_course_id_student_id").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("UUID định danh lượt ghi danh")
                .HasColumnName("id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.CourseId)
                .HasComment("UUID Course được ghi danh")
                .HasColumnName("course_id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.EnrolledAt)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .HasComment("Thời điểm Student ghi danh, UTC")
                .HasColumnName("enrolled_at");
            entity.Property(e => e.StudentId)
                .HasComment("UUID Student từ Student Service; logical reference")
                .HasColumnName("student_id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");

            entity.HasOne(d => d.Course).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("fk_enrollments_course_id");
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("lessons");

            entity.HasIndex(e => new { e.CourseId, e.DisplayOrder }, "uq_lessons_course_id_display_order").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("UUID định danh Lesson")
                .HasColumnName("id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.ContentMarkdown)
                .HasComment("Nội dung Markdown; có thể nhúng media")
                .HasColumnType("mediumtext")
                .HasColumnName("content_markdown");
            entity.Property(e => e.CourseId)
                .HasComment("UUID Course sở hữu Lesson")
                .HasColumnName("course_id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .HasComment("Thời điểm tạo Lesson, UTC")
                .HasColumnName("created_at");
            entity.Property(e => e.DisplayOrder)
                .HasComment("Thứ tự hiển thị Lesson trong Course")
                .HasColumnName("display_order");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasComment("Tiêu đề Lesson")
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasMaxLength(6)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .HasComment("Thời điểm cập nhật gần nhất, UTC")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Course).WithMany(p => p.Lessons)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("fk_lessons_course_id");
        });

        modelBuilder.Entity<LessonProgress>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("lesson_progresses");

            entity.HasIndex(e => new { e.StudentId, e.UpdatedAt }, "ix_lesson_progresses_student_id_updated_at").IsDescending(false, true);

            entity.HasIndex(e => new { e.LessonId, e.StudentId }, "uq_lesson_progresses_lesson_id_student_id").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("UUID định danh tiến độ học")
                .HasColumnName("id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.CompletedAt)
                .HasMaxLength(6)
                .HasComment("Thời điểm hoàn thành; null khi chưa hoàn thành")
                .HasColumnName("completed_at");
            entity.Property(e => e.LessonId)
                .HasComment("UUID Lesson được theo dõi tiến độ")
                .HasColumnName("lesson_id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.ProgressPercent)
                .HasComment("Phần trăm hoàn thành, từ 0 đến 100")
                .HasColumnType("decimal(5,2) unsigned")
                .HasColumnName("progress_percent");
            entity.Property(e => e.StudentId)
                .HasComment("UUID Student từ Student Service; logical reference")
                .HasColumnName("student_id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.UpdatedAt)
                .HasMaxLength(6)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .HasComment("Thời điểm cập nhật tiến độ gần nhất, UTC")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Lesson).WithMany(p => p.LessonProgresses)
                .HasForeignKey(d => d.LessonId)
                .HasConstraintName("fk_lesson_progresses_lesson_id");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
