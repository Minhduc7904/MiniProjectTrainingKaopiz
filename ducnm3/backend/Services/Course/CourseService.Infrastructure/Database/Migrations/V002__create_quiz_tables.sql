CREATE TABLE quizzes (
    id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID định danh Quiz',
    lesson_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID Lesson sở hữu Quiz',
    title VARCHAR(200) NOT NULL COMMENT 'Tiêu đề Quiz',
    description_markdown MEDIUMTEXT NULL COMMENT 'Mô tả Markdown; có thể nhúng media',
    display_order INT UNSIGNED NOT NULL COMMENT 'Thứ tự hiển thị Quiz trong Lesson',
    status VARCHAR(20) CHARACTER SET ascii NOT NULL COMMENT 'DRAFT | PUBLISHED; học viên chỉ thấy PUBLISHED',
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT 'Thời điểm tạo Quiz, UTC',
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6) COMMENT 'Thời điểm cập nhật gần nhất, UTC',
    CONSTRAINT pk_quizzes PRIMARY KEY (id),
    CONSTRAINT fk_quizzes_lesson_id FOREIGN KEY (lesson_id) REFERENCES lessons (id) ON DELETE CASCADE,
    CONSTRAINT uq_quizzes_lesson_id_display_order UNIQUE (lesson_id, display_order),
    CONSTRAINT chk_quizzes_status CHECK (status IN ('DRAFT', 'PUBLISHED'))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE quiz_questions (
    id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID định danh câu hỏi',
    quiz_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID Quiz sở hữu câu hỏi',
    question_markdown MEDIUMTEXT NOT NULL COMMENT 'Nội dung câu hỏi Markdown; có thể nhúng media và công thức',
    question_type VARCHAR(20) CHARACTER SET ascii NOT NULL COMMENT 'SINGLE_CHOICE | MULTIPLE_CHOICE',
    points DECIMAL(6, 2) NOT NULL COMMENT 'Điểm của câu hỏi, lớn hơn 0 và tối đa 100',
    display_order INT UNSIGNED NOT NULL COMMENT 'Thứ tự hiển thị câu hỏi trong Quiz',
    CONSTRAINT pk_quiz_questions PRIMARY KEY (id),
    CONSTRAINT fk_quiz_questions_quiz_id FOREIGN KEY (quiz_id) REFERENCES quizzes (id) ON DELETE CASCADE,
    CONSTRAINT uq_quiz_questions_quiz_id_display_order UNIQUE (quiz_id, display_order),
    CONSTRAINT chk_quiz_questions_question_type CHECK (question_type IN ('SINGLE_CHOICE', 'MULTIPLE_CHOICE')),
    CONSTRAINT chk_quiz_questions_points CHECK (points > 0 AND points <= 100)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE quiz_options (
    id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID định danh lựa chọn',
    question_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID câu hỏi sở hữu lựa chọn',
    option_markdown TEXT NOT NULL COMMENT 'Nội dung lựa chọn Markdown; có thể nhúng media và công thức',
    is_correct TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Lựa chọn này có phải đáp án đúng hay không',
    display_order INT UNSIGNED NOT NULL COMMENT 'Thứ tự hiển thị lựa chọn trong câu hỏi',
    CONSTRAINT pk_quiz_options PRIMARY KEY (id),
    CONSTRAINT fk_quiz_options_question_id FOREIGN KEY (question_id) REFERENCES quiz_questions (id) ON DELETE CASCADE,
    CONSTRAINT uq_quiz_options_question_id_display_order UNIQUE (question_id, display_order)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE quiz_attempts (
    id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID định danh lần làm bài',
    quiz_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID Quiz được làm',
    student_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID Student từ Student Service; logical reference',
    status VARCHAR(20) CHARACTER SET ascii NOT NULL COMMENT 'IN_PROGRESS | SUBMITTED',
    started_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT 'Thời điểm bắt đầu làm bài, UTC',
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6) COMMENT 'Thời điểm lưu nháp hoặc nộp gần nhất, UTC',
    submitted_at DATETIME(6) NULL COMMENT 'Thời điểm nộp bài; null khi đang làm dở',
    score DECIMAL(8, 2) NULL COMMENT 'Điểm đạt được; chỉ có khi đã nộp',
    max_score DECIMAL(8, 2) NULL COMMENT 'Tổng điểm tối đa tại lúc nộp; chỉ có khi đã nộp',
    open_marker CHAR(36) CHARACTER SET ascii COLLATE ascii_bin
        GENERATED ALWAYS AS (IF(status = 'IN_PROGRESS', student_id, NULL)) STORED
        COMMENT 'Chỉ có giá trị khi IN_PROGRESS; unique để mỗi Student có tối đa một bài làm dở cho mỗi Quiz',
    CONSTRAINT pk_quiz_attempts PRIMARY KEY (id),
    CONSTRAINT fk_quiz_attempts_quiz_id FOREIGN KEY (quiz_id) REFERENCES quizzes (id) ON DELETE CASCADE,
    CONSTRAINT uq_quiz_attempts_quiz_id_open_marker UNIQUE (quiz_id, open_marker),
    CONSTRAINT chk_quiz_attempts_status CHECK (status IN ('IN_PROGRESS', 'SUBMITTED')),
    CONSTRAINT chk_quiz_attempts_submitted CHECK (
        (status = 'IN_PROGRESS' AND submitted_at IS NULL AND score IS NULL AND max_score IS NULL)
        OR (status = 'SUBMITTED' AND submitted_at IS NOT NULL AND score IS NOT NULL AND max_score IS NOT NULL AND score <= max_score)
    ),
    INDEX ix_quiz_attempts_quiz_id_student_id_started_at (quiz_id, student_id, started_at DESC)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE quiz_attempt_answers (
    id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID định danh đáp án đã chọn',
    attempt_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID lần làm bài',
    question_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID câu hỏi được trả lời',
    selected_option_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID lựa chọn học viên đang chọn',
    CONSTRAINT pk_quiz_attempt_answers PRIMARY KEY (id),
    CONSTRAINT fk_quiz_attempt_answers_attempt_id FOREIGN KEY (attempt_id) REFERENCES quiz_attempts (id) ON DELETE CASCADE,
    CONSTRAINT fk_quiz_attempt_answers_question_id FOREIGN KEY (question_id) REFERENCES quiz_questions (id) ON DELETE CASCADE,
    CONSTRAINT fk_quiz_attempt_answers_selected_option_id FOREIGN KEY (selected_option_id) REFERENCES quiz_options (id) ON DELETE CASCADE,
    CONSTRAINT uq_quiz_attempt_answers_attempt_question_option UNIQUE (attempt_id, question_id, selected_option_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
