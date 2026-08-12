CREATE TABLE notification_jobs (
    id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID định danh batch job',
    course_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NULL COMMENT 'UUID Course liên quan; null nếu không gửi theo Course',
    title VARCHAR(200) NOT NULL COMMENT 'Tiêu đề thông báo',
    body_markdown MEDIUMTEXT NOT NULL COMMENT 'Nội dung Markdown; có thể nhúng media',
    target_scope VARCHAR(20) CHARACTER SET ascii NOT NULL COMMENT 'COURSE_ENROLLED | STUDENT_IDS | ALL_STUDENTS',
    created_by CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID admin tạo job; logical reference',
    status VARCHAR(20) CHARACTER SET ascii NOT NULL COMMENT 'PENDING | PROCESSING | COMPLETED | PARTIAL_FAILED | FAILED',
    total_count INT UNSIGNED NOT NULL DEFAULT 0 COMMENT 'Tổng recipient đã snapshot khi tạo job',
    processed_count INT UNSIGNED NOT NULL DEFAULT 0 COMMENT 'Số recipient worker đã xử lý',
    success_count INT UNSIGNED NOT NULL DEFAULT 0 COMMENT 'Số notification tạo thành công',
    failed_count INT UNSIGNED NOT NULL DEFAULT 0 COMMENT 'Số recipient thất bại sau retry',
    batch_size INT UNSIGNED NOT NULL COMMENT 'Số item xử lý trên mỗi chunk',
    started_at DATETIME(6) NULL COMMENT 'Thời điểm worker bắt đầu; null khi job chưa chạy',
    completed_at DATETIME(6) NULL COMMENT 'Thời điểm job kết thúc; null khi chưa hoàn tất',
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT 'Thời điểm tạo job, UTC',
    CONSTRAINT pk_notification_jobs PRIMARY KEY (id),
    CONSTRAINT chk_notification_jobs_target_scope CHECK (target_scope IN ('COURSE_ENROLLED', 'STUDENT_IDS', 'ALL_STUDENTS')),
    CONSTRAINT chk_notification_jobs_status CHECK (status IN ('PENDING', 'PROCESSING', 'COMPLETED', 'PARTIAL_FAILED', 'FAILED')),
    CONSTRAINT chk_notification_jobs_batch_size CHECK (batch_size > 0),
    INDEX ix_notification_jobs_status_created_at (status, created_at DESC)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE notifications (
    id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID định danh inbox item',
    recipient_student_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID Student sở hữu notification; logical reference',
    title VARCHAR(200) NOT NULL COMMENT 'Tiêu đề hiển thị trong inbox',
    body_markdown MEDIUMTEXT NOT NULL COMMENT 'Nội dung Markdown; media nhúng dùng URL Media Service',
    source_type VARCHAR(20) CHARACTER SET ascii NOT NULL COMMENT 'SINGLE | BULK',
    notification_job_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NULL COMMENT 'UUID notification_jobs.id; null với gửi đơn',
    created_by CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID admin hoặc system tạo notification; logical reference',
    status VARCHAR(20) CHARACTER SET ascii NOT NULL DEFAULT 'UNREAD' COMMENT 'UNREAD | READ',
    read_at DATETIME(6) NULL COMMENT 'Thời điểm recipient đánh dấu đã đọc; null khi UNREAD',
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT 'Thời điểm notification xuất hiện trong inbox, UTC',
    CONSTRAINT pk_notifications PRIMARY KEY (id),
    CONSTRAINT fk_notifications_notification_job_id FOREIGN KEY (notification_job_id) REFERENCES notification_jobs (id) ON DELETE SET NULL,
    CONSTRAINT chk_notifications_source_type CHECK (source_type IN ('SINGLE', 'BULK')),
    CONSTRAINT chk_notifications_status CHECK (status IN ('UNREAD', 'READ')),
    CONSTRAINT chk_notifications_read_at CHECK (
        (status = 'UNREAD' AND read_at IS NULL) OR
        (status = 'READ' AND read_at IS NOT NULL)
    ),
    CONSTRAINT uq_notifications_job_recipient UNIQUE (notification_job_id, recipient_student_id),
    INDEX ix_notifications_recipient_status_created_at (recipient_student_id, status, created_at DESC)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE notification_job_items (
    id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID định danh recipient trong batch',
    job_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID notification_jobs.id',
    student_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID Student nhận thông báo; logical reference',
    notification_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NULL COMMENT 'UUID notifications.id được tạo; null khi chưa thành công',
    status VARCHAR(20) CHARACTER SET ascii NOT NULL DEFAULT 'PENDING' COMMENT 'PENDING | PROCESSING | SUCCESS | RETRY | FAILED',
    retry_count INT UNSIGNED NOT NULL DEFAULT 0 COMMENT 'Số lần retry đã thực hiện',
    error_message TEXT NULL COMMENT 'Lỗi cuối cùng; null khi thành công',
    processed_at DATETIME(6) NULL COMMENT 'Thời điểm xử lý thành công hoặc thất bại cuối; null khi chưa xử lý',
    CONSTRAINT pk_notification_job_items PRIMARY KEY (id),
    CONSTRAINT fk_notification_job_items_job_id FOREIGN KEY (job_id) REFERENCES notification_jobs (id) ON DELETE CASCADE,
    CONSTRAINT fk_notification_job_items_notification_id FOREIGN KEY (notification_id) REFERENCES notifications (id) ON DELETE SET NULL,
    CONSTRAINT uq_notification_job_items_job_student UNIQUE (job_id, student_id),
    CONSTRAINT chk_notification_job_items_status CHECK (status IN ('PENDING', 'PROCESSING', 'SUCCESS', 'RETRY', 'FAILED')),
    INDEX ix_notification_job_items_job_status (job_id, status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
