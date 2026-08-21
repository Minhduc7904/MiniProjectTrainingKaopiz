-- Chuẩn hóa toàn bộ công việc bất đồng bộ của Media Worker vào một bảng vận hành chung.
DROP TABLE notification_media_usage_jobs;
DROP TABLE media_derivation_jobs;

CREATE TABLE media_background_jobs (
    id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL
        COMMENT 'UUID định danh background job',
    job_type VARCHAR(48) CHARACTER SET ascii NOT NULL
        COMMENT 'THUMBNAIL_DERIVATION | MARKDOWN_USAGE_SYNC | MEDIA_USAGE_DELETE | NOTIFICATION_USAGE',
    subject_type VARCHAR(48) CHARACTER SET ascii NOT NULL
        COMMENT 'Loại resource chịu tác động bởi job',
    subject_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL
        COMMENT 'UUID resource chịu tác động bởi job',
    correlation_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NULL
        COMMENT 'Correlation logical reference, ví dụ Notification Batch ID',
    deduplication_key VARCHAR(128) CHARACTER SET ascii NULL
        COMMENT 'Khóa idempotency nghiệp vụ, chỉ dùng khi một subject có tối đa một job active',
    status VARCHAR(20) CHARACTER SET ascii NOT NULL DEFAULT 'QUEUED'
        COMMENT 'QUEUED | PROCESSING | COMPLETED | PARTIAL_FAILED | FAILED',
    payload_json JSON NOT NULL
        COMMENT 'Payload versioned, không chứa secret; Notification batch chứa các chunk retry được',
    expected_item_count INT UNSIGNED NULL
        COMMENT 'Tổng item được source chốt cho job aggregate',
    processed_item_count INT UNSIGNED NOT NULL DEFAULT 0
        COMMENT 'Số item hoàn thành thành công',
    failed_item_count INT UNSIGNED NOT NULL DEFAULT 0
        COMMENT 'Số item đã hết transport retry và thất bại',
    attempt_count INT UNSIGNED NOT NULL DEFAULT 0
        COMMENT 'Tổng lần worker bắt đầu xử lý hoặc retry thủ công',
    last_error VARCHAR(500) NULL
        COMMENT 'Lỗi an toàn gần nhất, không chứa stack trace hoặc dữ liệu bí mật',
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)
        COMMENT 'Thời điểm tạo job, UTC',
    started_at DATETIME(6) NULL
        COMMENT 'Thời điểm worker bắt đầu lần xử lý đầu tiên, UTC',
    completed_at DATETIME(6) NULL
        COMMENT 'Thời điểm job đạt trạng thái terminal, UTC',
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)
        ON UPDATE CURRENT_TIMESTAMP(6)
        COMMENT 'Thời điểm job được cập nhật gần nhất, UTC',
    CONSTRAINT pk_media_background_jobs PRIMARY KEY (id),
    CONSTRAINT uq_media_background_jobs_deduplication_key UNIQUE (deduplication_key),
    CONSTRAINT chk_media_background_jobs_type CHECK (job_type IN (
        'THUMBNAIL_DERIVATION',
        'MARKDOWN_USAGE_SYNC',
        'MEDIA_USAGE_DELETE',
        'NOTIFICATION_USAGE'
    )),
    CONSTRAINT chk_media_background_jobs_status CHECK (status IN (
        'QUEUED', 'PROCESSING', 'COMPLETED', 'PARTIAL_FAILED', 'FAILED'
    )),
    INDEX ix_media_background_jobs_status_updated_at (status, updated_at),
    INDEX ix_media_background_jobs_type_subject (job_type, subject_type, subject_id),
    INDEX ix_media_background_jobs_correlation_id (correlation_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
