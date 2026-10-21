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

    public virtual DbSet<LessonAiJob> LessonAiJobs { get; set; }

    public virtual DbSet<LessonAttachmentText> LessonAttachmentTexts { get; set; }

    public virtual DbSet<LessonProgress> LessonProgresses { get; set; }

    public virtual DbSet<Quiz> Quizzes { get; set; }

    public virtual DbSet<QuizAttempt> QuizAttempts { get; set; }

    public virtual DbSet<QuizAttemptAnswer> QuizAttemptAnswers { get; set; }

    public virtual DbSet<QuizOption> QuizOptions { get; set; }

    public virtual DbSet<QuizQuestion> QuizQuestions { get; set; }

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

        modelBuilder.Entity<LessonAiJob>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("lesson_ai_jobs");

            entity.HasIndex(e => new { e.LessonId, e.CreatedAt }, "ix_lesson_ai_jobs_lesson_id_created_at").IsDescending(false, true);

            entity.HasIndex(e => new { e.Status, e.CreatedAt }, "ix_lesson_ai_jobs_status_created_at");

            entity.Property(e => e.Id)
                .HasComment("UUID định danh job AI")
                .HasColumnName("id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.CourseId)
                .HasComment("UUID Course của Lesson, dùng khi gọi handler cập nhật Lesson")
                .HasColumnName("course_id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .HasComment("Thời điểm tạo job, UTC")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasComment("UUID admin tạo job; dùng làm actor khi gọi AI Service và ghi dữ liệu")
                .HasColumnName("created_by")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.ErrorCode)
                .HasMaxLength(60)
                .HasComment("Mã lỗi an toàn khi FAILED")
                .HasColumnName("error_code")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.ErrorMessage)
                .HasMaxLength(500)
                .HasComment("Thông báo lỗi an toàn hiển thị cho admin")
                .HasColumnName("error_message");
            entity.Property(e => e.FinishedAt)
                .HasMaxLength(6)
                .HasComment("Thời điểm job kết thúc (COMPLETED hoặc FAILED), UTC")
                .HasColumnName("finished_at");
            entity.Property(e => e.JobType)
                .HasMaxLength(30)
                .HasComment("SYNTHESIZE_LESSON | GENERATE_QUIZ")
                .HasColumnName("job_type")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.LessonId)
                .HasComment("UUID Lesson mà job ghi kết quả vào")
                .HasColumnName("lesson_id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.LockedUntil)
                .HasMaxLength(6)
                .HasComment("Hạn lease của worker đang chạy job, UTC; quá hạn thì worker khác nhận lại")
                .HasColumnName("locked_until");
            entity.Property(e => e.MediaIdsJson)
                .HasComment("Mảng UUID media (tài liệu) mà admin đã chọn làm nguồn")
                .HasColumnType("json")
                .HasColumnName("media_ids_json");
            entity.Property(e => e.OverwriteContent)
                .HasComment("Admin đã xác nhận cho ghi đè nội dung Lesson đang có")
                .HasColumnName("overwrite_content");
            entity.Property(e => e.PreviousContentMarkdown)
                .HasComment("Nội dung Lesson trước khi job ghi đè, để khôi phục")
                .HasColumnType("mediumtext")
                .HasColumnName("previous_content_markdown");
            entity.Property(e => e.QuestionCount)
                .HasComment("Số câu hỏi yêu cầu khi GENERATE_QUIZ; NULL với SYNTHESIZE_LESSON")
                .HasColumnName("question_count");
            entity.Property(e => e.ResultQuizId)
                .HasComment("UUID Quiz DRAFT do job tạo; chỉ có khi GENERATE_QUIZ hoàn tất")
                .HasColumnName("result_quiz_id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.StartedAt)
                .HasMaxLength(6)
                .HasComment("Thời điểm worker bắt đầu chạy, UTC")
                .HasColumnName("started_at");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasComment("PENDING | RUNNING | COMPLETED | FAILED")
                .HasColumnName("status")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.StepCount)
                .HasComment("Số lượt gọi model của job")
                .HasColumnName("step_count");
            entity.Property(e => e.UpdatedAt)
                .HasMaxLength(6)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .HasComment("Thời điểm cập nhật gần nhất, UTC")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Lesson).WithMany(p => p.LessonAiJobs)
                .HasForeignKey(d => d.LessonId)
                .HasConstraintName("fk_lesson_ai_jobs_lesson_id");
        });

        modelBuilder.Entity<LessonAttachmentText>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("lesson_attachment_texts");

            entity.HasIndex(e => new { e.Status, e.CreatedAt }, "ix_lesson_attachment_texts_status_created_at");

            entity.HasIndex(e => new { e.LessonId, e.MediaId }, "uq_lesson_attachment_texts_lesson_id_media_id").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("UUID định danh bản trích xuất")
                .HasColumnName("id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.Attempts)
                .HasComment("Số lần worker đã nhận trích xuất tài liệu này")
                .HasColumnName("attempts");
            entity.Property(e => e.ContentType)
                .HasMaxLength(150)
                .HasComment("MIME type của tệp tại lúc đưa vào hàng chờ")
                .HasColumnName("content_type")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .HasComment("Thời điểm vào hàng chờ, UTC")
                .HasColumnName("created_at");
            entity.Property(e => e.ErrorCode)
                .HasMaxLength(60)
                .HasComment("Mã lỗi an toàn khi FAILED hoặc UNSUPPORTED")
                .HasColumnName("error_code")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.ErrorMessage)
                .HasMaxLength(500)
                .HasComment("Thông báo lỗi an toàn hiển thị cho admin")
                .HasColumnName("error_message");
            entity.Property(e => e.ExtractedText)
                .HasComment("Văn bản (Markdown) đã trích xuất; chỉ có khi EXTRACTED")
                .HasColumnType("mediumtext")
                .HasColumnName("extracted_text");
            entity.Property(e => e.FileName)
                .HasMaxLength(255)
                .HasComment("Tên tệp gốc tại lúc đưa vào hàng chờ, để hiển thị")
                .HasColumnName("file_name");
            entity.Property(e => e.LessonId)
                .HasComment("UUID Lesson sở hữu tài liệu")
                .HasColumnName("lesson_id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.LockedUntil)
                .HasMaxLength(6)
                .HasComment("Hạn lease của worker đang trích xuất, UTC; quá hạn thì worker khác nhận lại")
                .HasColumnName("locked_until");
            entity.Property(e => e.MediaId)
                .HasComment("UUID media đính kèm từ Media Service; logical reference")
                .HasColumnName("media_id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.RequestedBy)
                .HasComment("UUID admin đã yêu cầu; dùng làm actor khi gọi AI Service")
                .HasColumnName("requested_by")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasComment("PENDING | EXTRACTING | EXTRACTED | FAILED | UNSUPPORTED")
                .HasColumnName("status")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.UpdatedAt)
                .HasMaxLength(6)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .HasComment("Thời điểm cập nhật gần nhất, UTC")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Lesson).WithMany(p => p.LessonAttachmentTexts)
                .HasForeignKey(d => d.LessonId)
                .HasConstraintName("fk_lesson_attachment_texts_lesson_id");
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

        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("quizzes");

            entity.HasIndex(e => new { e.LessonId, e.DisplayOrder }, "uq_quizzes_lesson_id_display_order").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("UUID định danh Quiz")
                .HasColumnName("id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .HasComment("Thời điểm tạo Quiz, UTC")
                .HasColumnName("created_at");
            entity.Property(e => e.DescriptionMarkdown)
                .HasComment("Mô tả Markdown; có thể nhúng media")
                .HasColumnType("mediumtext")
                .HasColumnName("description_markdown");
            entity.Property(e => e.DisplayOrder)
                .HasComment("Thứ tự hiển thị Quiz trong Lesson")
                .HasColumnName("display_order");
            entity.Property(e => e.LessonId)
                .HasComment("UUID Lesson sở hữu Quiz")
                .HasColumnName("lesson_id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasComment("DRAFT | PUBLISHED; học viên chỉ thấy PUBLISHED")
                .HasColumnName("status")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasComment("Tiêu đề Quiz")
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasMaxLength(6)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .HasComment("Thời điểm cập nhật gần nhất, UTC")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Lesson).WithMany(p => p.Quizzes)
                .HasForeignKey(d => d.LessonId)
                .HasConstraintName("fk_quizzes_lesson_id");
        });

        modelBuilder.Entity<QuizAttempt>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("quiz_attempts");

            entity.HasIndex(e => new { e.QuizId, e.StudentId, e.StartedAt }, "ix_quiz_attempts_quiz_id_student_id_started_at").IsDescending(false, false, true);

            entity.HasIndex(e => new { e.QuizId, e.OpenMarker }, "uq_quiz_attempts_quiz_id_open_marker").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("UUID định danh lần làm bài")
                .HasColumnName("id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.MaxScore)
                .HasPrecision(8, 2)
                .HasComment("Tổng điểm tối đa tại lúc nộp; chỉ có khi đã nộp")
                .HasColumnName("max_score");
            entity.Property(e => e.OpenMarker)
                .HasComputedColumnSql("if((`status` = _utf8mb4'IN_PROGRESS'),`student_id`,NULL)", true)
                .HasComment("Chỉ có giá trị khi IN_PROGRESS; unique để mỗi Student có tối đa một bài làm dở cho mỗi Quiz")
                .HasColumnName("open_marker")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.QuizId)
                .HasComment("UUID Quiz được làm")
                .HasColumnName("quiz_id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.Score)
                .HasPrecision(8, 2)
                .HasComment("Điểm đạt được; chỉ có khi đã nộp")
                .HasColumnName("score");
            entity.Property(e => e.StartedAt)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .HasComment("Thời điểm bắt đầu làm bài, UTC")
                .HasColumnName("started_at");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasComment("IN_PROGRESS | SUBMITTED")
                .HasColumnName("status")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.StudentId)
                .HasComment("UUID Student từ Student Service; logical reference")
                .HasColumnName("student_id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.SubmittedAt)
                .HasMaxLength(6)
                .HasComment("Thời điểm nộp bài; null khi đang làm dở")
                .HasColumnName("submitted_at");
            entity.Property(e => e.UpdatedAt)
                .HasMaxLength(6)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .HasComment("Thời điểm lưu nháp hoặc nộp gần nhất, UTC")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Quiz).WithMany(p => p.QuizAttempts)
                .HasForeignKey(d => d.QuizId)
                .HasConstraintName("fk_quiz_attempts_quiz_id");
        });

        modelBuilder.Entity<QuizAttemptAnswer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("quiz_attempt_answers");

            entity.HasIndex(e => e.QuestionId, "fk_quiz_attempt_answers_question_id");

            entity.HasIndex(e => e.SelectedOptionId, "fk_quiz_attempt_answers_selected_option_id");

            entity.HasIndex(e => new { e.AttemptId, e.QuestionId, e.SelectedOptionId }, "uq_quiz_attempt_answers_attempt_question_option").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("UUID định danh đáp án đã chọn")
                .HasColumnName("id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.AttemptId)
                .HasComment("UUID lần làm bài")
                .HasColumnName("attempt_id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.QuestionId)
                .HasComment("UUID câu hỏi được trả lời")
                .HasColumnName("question_id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.SelectedOptionId)
                .HasComment("UUID lựa chọn học viên đang chọn")
                .HasColumnName("selected_option_id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");

            entity.HasOne(d => d.Attempt).WithMany(p => p.QuizAttemptAnswers)
                .HasForeignKey(d => d.AttemptId)
                .HasConstraintName("fk_quiz_attempt_answers_attempt_id");

            entity.HasOne(d => d.Question).WithMany(p => p.QuizAttemptAnswers)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("fk_quiz_attempt_answers_question_id");

            entity.HasOne(d => d.SelectedOption).WithMany(p => p.QuizAttemptAnswers)
                .HasForeignKey(d => d.SelectedOptionId)
                .HasConstraintName("fk_quiz_attempt_answers_selected_option_id");
        });

        modelBuilder.Entity<QuizOption>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("quiz_options");

            entity.HasIndex(e => new { e.QuestionId, e.DisplayOrder }, "uq_quiz_options_question_id_display_order").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("UUID định danh lựa chọn")
                .HasColumnName("id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.DisplayOrder)
                .HasComment("Thứ tự hiển thị lựa chọn trong câu hỏi")
                .HasColumnName("display_order");
            entity.Property(e => e.IsCorrect)
                .HasComment("Lựa chọn này có phải đáp án đúng hay không")
                .HasColumnName("is_correct");
            entity.Property(e => e.OptionMarkdown)
                .HasComment("Nội dung lựa chọn Markdown; có thể nhúng media và công thức")
                .HasColumnType("text")
                .HasColumnName("option_markdown");
            entity.Property(e => e.QuestionId)
                .HasComment("UUID câu hỏi sở hữu lựa chọn")
                .HasColumnName("question_id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");

            entity.HasOne(d => d.Question).WithMany(p => p.QuizOptions)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("fk_quiz_options_question_id");
        });

        modelBuilder.Entity<QuizQuestion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("quiz_questions");

            entity.HasIndex(e => new { e.QuizId, e.DisplayOrder }, "uq_quiz_questions_quiz_id_display_order").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("UUID định danh câu hỏi")
                .HasColumnName("id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.DisplayOrder)
                .HasComment("Thứ tự hiển thị câu hỏi trong Quiz")
                .HasColumnName("display_order");
            entity.Property(e => e.Points)
                .HasPrecision(6, 2)
                .HasComment("Điểm của câu hỏi, lớn hơn 0 và tối đa 100")
                .HasColumnName("points");
            entity.Property(e => e.QuestionMarkdown)
                .HasComment("Nội dung câu hỏi Markdown; có thể nhúng media và công thức")
                .HasColumnType("mediumtext")
                .HasColumnName("question_markdown");
            entity.Property(e => e.QuestionType)
                .HasMaxLength(20)
                .HasComment("SINGLE_CHOICE | MULTIPLE_CHOICE")
                .HasColumnName("question_type")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.QuizId)
                .HasComment("UUID Quiz sở hữu câu hỏi")
                .HasColumnName("quiz_id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");

            entity.HasOne(d => d.Quiz).WithMany(p => p.QuizQuestions)
                .HasForeignKey(d => d.QuizId)
                .HasConstraintName("fk_quiz_questions_quiz_id");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
