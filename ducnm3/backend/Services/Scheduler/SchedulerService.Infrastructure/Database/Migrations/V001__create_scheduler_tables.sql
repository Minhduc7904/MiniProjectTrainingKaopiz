CREATE TABLE background_jobs (
    id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID định danh cấu hình background job',
    job_key VARCHAR(100) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'Khóa ổn định, duy nhất dùng để đăng ký và tra cứu job',
    name VARCHAR(200) NOT NULL COMMENT 'Tên job hiển thị cho vận hành',
    job_type VARCHAR(100) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'Loại handler tương lai, ví dụ NOTIFICATION_BATCH_DISPATCH hoặc MEDIA_UNUSED_CLEANUP',
    target_service VARCHAR(100) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'Service sở hữu nghiệp vụ sẽ được gọi trong phase execution tương lai',
    schedule_type VARCHAR(20) CHARACTER SET ascii NOT NULL COMMENT 'MANUAL | CRON',
    cron_expression VARCHAR(120) CHARACTER SET ascii NULL COMMENT 'Biểu thức CRON theo UTC; null với MANUAL',
    payload_json JSON NULL COMMENT 'Cấu hình đầu vào chung; không lưu recipient list hoặc dữ liệu domain lớn',
    status VARCHAR(20) CHARACTER SET ascii NOT NULL DEFAULT 'ACTIVE' COMMENT 'ACTIVE | PAUSED | DISABLED',
    allow_concurrent TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Cho phép nhiều run đồng thời của cùng job hay không',
    max_retry_count INT UNSIGNED NOT NULL DEFAULT 0 COMMENT 'Số lần retry tối đa dành cho execution phase tương lai',
    timeout_seconds INT UNSIGNED NOT NULL DEFAULT 300 COMMENT 'Thời gian chạy tối đa trước khi đánh dấu timeout',
    next_run_at DATETIME(6) NULL COMMENT 'Thời điểm UTC chạy CRON kế tiếp; null khi chưa tính hoặc là MANUAL',
    created_by CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NULL COMMENT 'UUID actor tạo job; null với system-defined job',
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT 'Thời điểm tạo job, UTC',
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6) COMMENT 'Thời điểm cập nhật job gần nhất, UTC',
    CONSTRAINT pk_background_jobs PRIMARY KEY (id),
    CONSTRAINT uq_background_jobs_job_key UNIQUE (job_key),
    CONSTRAINT chk_background_jobs_schedule_type CHECK (schedule_type IN ('MANUAL', 'CRON')),
    CONSTRAINT chk_background_jobs_schedule_config CHECK (
        (schedule_type = 'MANUAL' AND cron_expression IS NULL) OR
        (schedule_type = 'CRON' AND cron_expression IS NOT NULL)
    ),
    CONSTRAINT chk_background_jobs_status CHECK (status IN ('ACTIVE', 'PAUSED', 'DISABLED')),
    CONSTRAINT chk_background_jobs_timeout_seconds CHECK (timeout_seconds > 0),
    INDEX ix_background_jobs_status_next_run_at (status, next_run_at),
    INDEX ix_background_jobs_target_service_job_type (target_service, job_type)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE background_job_runs (
    id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID định danh một lần thực thi job',
    background_job_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID background_jobs.id trong Scheduler database',
    trigger_type VARCHAR(20) CHARACTER SET ascii NOT NULL COMMENT 'MANUAL | CRON | RETRY',
    idempotency_key VARCHAR(128) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'Khóa chống tạo trùng run cho cùng job',
    payload_snapshot_json JSON NULL COMMENT 'Snapshot payload tại thời điểm tạo run để phục vụ audit',
    attempt_number INT UNSIGNED NOT NULL DEFAULT 1 COMMENT 'Lần thử hiện tại, bắt đầu từ 1',
    status VARCHAR(20) CHARACTER SET ascii NOT NULL DEFAULT 'QUEUED' COMMENT 'QUEUED | RUNNING | SUCCEEDED | FAILED | CANCELLED | TIMED_OUT | SKIPPED',
    scheduled_at DATETIME(6) NOT NULL COMMENT 'Thời điểm UTC run được lên lịch hoặc manual trigger',
    started_at DATETIME(6) NULL COMMENT 'Thời điểm worker bắt đầu xử lý, UTC',
    finished_at DATETIME(6) NULL COMMENT 'Thời điểm worker kết thúc xử lý, UTC',
    worker_instance VARCHAR(200) CHARACTER SET ascii NULL COMMENT 'Định danh worker instance thực thi run',
    correlation_id VARCHAR(128) CHARACTER SET ascii COLLATE ascii_bin NULL COMMENT 'Correlation ID dùng nối log giữa Scheduler và target service',
    error_code VARCHAR(100) CHARACTER SET ascii NULL COMMENT 'Mã lỗi ổn định cuối cùng; null khi chưa lỗi',
    error_message TEXT NULL COMMENT 'Thông tin lỗi an toàn cho vận hành; không chứa credential',
    output_json JSON NULL COMMENT 'Kết quả tóm tắt của run; không dùng thay domain database',
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT 'Thời điểm tạo run, UTC',
    CONSTRAINT pk_background_job_runs PRIMARY KEY (id),
    CONSTRAINT fk_background_job_runs_background_job_id FOREIGN KEY (background_job_id) REFERENCES background_jobs (id) ON DELETE RESTRICT,
    CONSTRAINT uq_background_job_runs_job_idempotency UNIQUE (background_job_id, idempotency_key),
    CONSTRAINT chk_background_job_runs_trigger_type CHECK (trigger_type IN ('MANUAL', 'CRON', 'RETRY')),
    CONSTRAINT chk_background_job_runs_attempt_number CHECK (attempt_number > 0),
    CONSTRAINT chk_background_job_runs_status CHECK (status IN ('QUEUED', 'RUNNING', 'SUCCEEDED', 'FAILED', 'CANCELLED', 'TIMED_OUT', 'SKIPPED')),
    CONSTRAINT chk_background_job_runs_timestamps CHECK (
        finished_at IS NULL OR started_at IS NULL OR finished_at >= started_at
    ),
    INDEX ix_background_job_runs_status_scheduled_at (status, scheduled_at),
    INDEX ix_background_job_runs_job_created_at (background_job_id, created_at DESC)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
