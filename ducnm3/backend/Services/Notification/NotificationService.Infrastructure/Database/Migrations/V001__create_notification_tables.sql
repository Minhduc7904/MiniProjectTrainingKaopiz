CREATE TABLE notification_batches (
    id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID định danh yêu cầu gửi notification hàng loạt',
    course_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NULL COMMENT 'UUID Course liên quan; null nếu không gửi theo Course',
    title VARCHAR(200) NOT NULL COMMENT 'Tiêu đề notification dùng cho toàn batch',
    body_markdown MEDIUMTEXT NOT NULL COMMENT 'Nội dung Markdown dùng cho toàn batch; có thể nhúng media',
    target_scope VARCHAR(20) CHARACTER SET ascii NOT NULL COMMENT 'COURSE_ENROLLED | STUDENT_IDS | ALL_STUDENTS',
    created_by CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID admin tạo batch; logical reference',
    status VARCHAR(20) CHARACTER SET ascii NOT NULL COMMENT 'PENDING | PROCESSING | COMPLETED | PARTIAL_FAILED | FAILED',
    total_count INT UNSIGNED NOT NULL DEFAULT 0 COMMENT 'Tổng recipient đã snapshot khi tạo batch',
    processed_count INT UNSIGNED NOT NULL DEFAULT 0 COMMENT 'Số recipient đã được xử lý',
    success_count INT UNSIGNED NOT NULL DEFAULT 0 COMMENT 'Số notification inbox tạo thành công',
    failed_count INT UNSIGNED NOT NULL DEFAULT 0 COMMENT 'Số recipient thất bại sau retry',
    batch_size INT UNSIGNED NOT NULL COMMENT 'Số item nghiệp vụ xử lý trên mỗi chunk',
    started_at DATETIME(6) NULL COMMENT 'Thời điểm bắt đầu xử lý batch, UTC',
    completed_at DATETIME(6) NULL COMMENT 'Thời điểm kết thúc batch, UTC',
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT 'Thời điểm tạo batch, UTC',
    CONSTRAINT pk_notification_batches PRIMARY KEY (id),
    CONSTRAINT chk_notification_batches_target_scope CHECK (target_scope IN ('COURSE_ENROLLED', 'STUDENT_IDS', 'ALL_STUDENTS')),
    CONSTRAINT chk_notification_batches_status CHECK (status IN ('PENDING', 'PROCESSING', 'COMPLETED', 'PARTIAL_FAILED', 'FAILED')),
    CONSTRAINT chk_notification_batches_batch_size CHECK (batch_size > 0),
    CONSTRAINT chk_notification_batches_counts CHECK (
        processed_count <= total_count AND
        success_count + failed_count <= processed_count
    ),
    INDEX ix_notification_batches_status_created_at (status, created_at DESC)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE notifications (
    id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID định danh inbox item',
    recipient_student_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID Student sở hữu notification; logical reference',
    title VARCHAR(200) NOT NULL COMMENT 'Tiêu đề hiển thị trong inbox',
    body_markdown MEDIUMTEXT NOT NULL COMMENT 'Nội dung Markdown; media nhúng dùng URL Media Service',
    source_type VARCHAR(20) CHARACTER SET ascii NOT NULL COMMENT 'SINGLE | BULK',
    notification_batch_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NULL COMMENT 'UUID notification_batches.id; null với gửi đơn',
    created_by CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID admin hoặc system tạo notification; logical reference',
    status VARCHAR(20) CHARACTER SET ascii NOT NULL DEFAULT 'UNREAD' COMMENT 'UNREAD | READ',
    read_at DATETIME(6) NULL COMMENT 'Thời điểm recipient đánh dấu đã đọc; null khi UNREAD',
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT 'Thời điểm notification xuất hiện trong inbox, UTC',
    CONSTRAINT pk_notifications PRIMARY KEY (id),
    CONSTRAINT fk_notifications_notification_batch_id FOREIGN KEY (notification_batch_id) REFERENCES notification_batches (id) ON DELETE RESTRICT,
    CONSTRAINT chk_notifications_source_type CHECK (source_type IN ('SINGLE', 'BULK')),
    CONSTRAINT chk_notifications_source_reference CHECK (
        (source_type = 'SINGLE' AND notification_batch_id IS NULL) OR
        (source_type = 'BULK' AND notification_batch_id IS NOT NULL)
    ),
    CONSTRAINT chk_notifications_status CHECK (status IN ('UNREAD', 'READ')),
    CONSTRAINT chk_notifications_read_at CHECK (
        (status = 'UNREAD' AND read_at IS NULL) OR
        (status = 'READ' AND read_at IS NOT NULL)
    ),
    CONSTRAINT uq_notifications_batch_recipient UNIQUE (notification_batch_id, recipient_student_id),
    INDEX ix_notifications_recipient_status_created_at (recipient_student_id, status, created_at DESC)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE notification_batch_items (
    id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID định danh recipient trong batch',
    batch_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID notification_batches.id',
    student_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID Student nhận notification; logical reference',
    notification_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NULL COMMENT 'UUID notifications.id được tạo; null khi chưa thành công',
    status VARCHAR(20) CHARACTER SET ascii NOT NULL DEFAULT 'PENDING' COMMENT 'PENDING | PROCESSING | SUCCESS | RETRY | FAILED',
    retry_count INT UNSIGNED NOT NULL DEFAULT 0 COMMENT 'Số lần retry item nghiệp vụ đã thực hiện',
    error_message TEXT NULL COMMENT 'Lỗi cuối cùng; null khi thành công',
    processed_at DATETIME(6) NULL COMMENT 'Thời điểm xử lý thành công hoặc thất bại cuối, UTC',
    CONSTRAINT pk_notification_batch_items PRIMARY KEY (id),
    CONSTRAINT fk_notification_batch_items_batch_id FOREIGN KEY (batch_id) REFERENCES notification_batches (id) ON DELETE CASCADE,
    CONSTRAINT fk_notification_batch_items_notification_id FOREIGN KEY (notification_id) REFERENCES notifications (id) ON DELETE SET NULL,
    CONSTRAINT uq_notification_batch_items_batch_student UNIQUE (batch_id, student_id),
    CONSTRAINT chk_notification_batch_items_status CHECK (status IN ('PENDING', 'PROCESSING', 'SUCCESS', 'RETRY', 'FAILED')),
    INDEX ix_notification_batch_items_batch_status (batch_id, status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
