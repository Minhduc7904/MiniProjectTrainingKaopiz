CREATE TABLE courses (
    id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID định danh Course',
    name VARCHAR(200) NOT NULL COMMENT 'Tên Course hiển thị cho người dùng',
    description_markdown MEDIUMTEXT NULL COMMENT 'Nội dung mô tả Markdown; có thể nhúng media',
    status VARCHAR(20) CHARACTER SET ascii NOT NULL COMMENT 'DRAFT | PUBLISHED | ARCHIVED',
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT 'Thời điểm tạo Course, UTC',
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6) COMMENT 'Thời điểm cập nhật gần nhất, UTC',
    CONSTRAINT pk_courses PRIMARY KEY (id),
    CONSTRAINT chk_courses_status CHECK (status IN ('DRAFT', 'PUBLISHED', 'ARCHIVED')),
    INDEX ix_courses_status_created_at (status, created_at DESC)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE lessons (
    id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID định danh Lesson',
    course_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID Course sở hữu Lesson',
    title VARCHAR(200) NOT NULL COMMENT 'Tiêu đề Lesson',
    display_order INT UNSIGNED NOT NULL COMMENT 'Thứ tự hiển thị Lesson trong Course',
    content_markdown MEDIUMTEXT NULL COMMENT 'Nội dung Markdown; có thể nhúng media',
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT 'Thời điểm tạo Lesson, UTC',
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6) COMMENT 'Thời điểm cập nhật gần nhất, UTC',
    CONSTRAINT pk_lessons PRIMARY KEY (id),
    CONSTRAINT fk_lessons_course_id FOREIGN KEY (course_id) REFERENCES courses (id) ON DELETE CASCADE,
    CONSTRAINT uq_lessons_course_id_display_order UNIQUE (course_id, display_order)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE enrollments (
    id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID định danh lượt ghi danh',
    course_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID Course được ghi danh',
    student_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID Student từ Student Service; logical reference',
    enrolled_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT 'Thời điểm Student ghi danh, UTC',
    CONSTRAINT pk_enrollments PRIMARY KEY (id),
    CONSTRAINT fk_enrollments_course_id FOREIGN KEY (course_id) REFERENCES courses (id) ON DELETE CASCADE,
    CONSTRAINT uq_enrollments_course_id_student_id UNIQUE (course_id, student_id),
    INDEX ix_enrollments_student_id_enrolled_at (student_id, enrolled_at DESC)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE lesson_progresses (
    id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID định danh tiến độ học',
    lesson_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID Lesson được theo dõi tiến độ',
    student_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID Student từ Student Service; logical reference',
    progress_percent DECIMAL(5, 2) UNSIGNED NOT NULL DEFAULT 0 COMMENT 'Phần trăm hoàn thành, từ 0 đến 100',
    completed_at DATETIME(6) NULL COMMENT 'Thời điểm hoàn thành; null khi chưa hoàn thành',
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6) COMMENT 'Thời điểm cập nhật tiến độ gần nhất, UTC',
    CONSTRAINT pk_lesson_progresses PRIMARY KEY (id),
    CONSTRAINT fk_lesson_progresses_lesson_id FOREIGN KEY (lesson_id) REFERENCES lessons (id) ON DELETE CASCADE,
    CONSTRAINT uq_lesson_progresses_lesson_id_student_id UNIQUE (lesson_id, student_id),
    CONSTRAINT chk_lesson_progresses_progress_percent CHECK (progress_percent BETWEEN 0 AND 100),
    INDEX ix_lesson_progresses_student_id_updated_at (student_id, updated_at DESC)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
