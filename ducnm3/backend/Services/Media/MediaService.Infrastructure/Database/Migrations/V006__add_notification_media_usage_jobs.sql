-- Theo dõi tiến độ background job đăng ký Media Usage cho từng Notification Batch.
CREATE TABLE notification_media_usage_jobs (
    id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL
        COMMENT 'UUID job; dùng cùng UUID với Notification Batch nguồn',
    status VARCHAR(20) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL DEFAULT 'PENDING'
        COMMENT 'PENDING | PROCESSING | COMPLETED | PARTIAL_FAILED | FAILED',
    expected_usage_count INT UNSIGNED NULL
        COMMENT 'Tổng usage Notification Service chốt sau khi delivery terminal',
    processed_usage_count INT UNSIGNED NOT NULL DEFAULT 0
        COMMENT 'Số usage đã được Media Worker đăng ký thành công',
    failed_usage_count INT UNSIGNED NOT NULL DEFAULT 0
        COMMENT 'Số usage thuộc command đã hết retry và thất bại',
    last_error VARCHAR(500) NULL
        COMMENT 'Lỗi an toàn gần nhất; không chứa stack trace hoặc dữ liệu bí mật',
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)
        COMMENT 'Thời điểm Media Worker tiếp nhận job',
    started_at DATETIME(6) NULL
        COMMENT 'Thời điểm chunk Media Usage đầu tiên bắt đầu được ghi nhận',
    completed_at DATETIME(6) NULL
        COMMENT 'Thời điểm tổng processed và failed đạt expected count',
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)
        COMMENT 'Thời điểm job thay đổi gần nhất',
    CONSTRAINT pk_notification_media_usage_jobs PRIMARY KEY (id),
    CONSTRAINT chk_notification_media_usage_jobs_status CHECK (
        status IN ('PENDING', 'PROCESSING', 'COMPLETED', 'PARTIAL_FAILED', 'FAILED')),
    INDEX ix_notification_media_usage_jobs_status_updated_at (status, updated_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
