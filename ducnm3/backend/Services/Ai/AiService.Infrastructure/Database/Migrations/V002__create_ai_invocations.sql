CREATE TABLE ai_invocations (
    id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID định danh lần gọi model',
    task_name VARCHAR(50) CHARACTER SET ascii NOT NULL COMMENT 'Khóa binding đã gọi, ví dụ document-extraction; logical reference tới ai_service_bindings.service_name',
    model_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID AI model đã dùng; logical reference, không có khóa ngoại để model xóa được mà vẫn giữ nhật ký',
    provider_type VARCHAR(20) CHARACTER SET ascii NOT NULL COMMENT 'Loại provider tại thời điểm gọi: OPENAI | GEMINI | ANTHROPIC | MISTRAL | CUSTOM',
    model_name VARCHAR(100) NOT NULL COMMENT 'Tên model gửi cho provider tại thời điểm gọi',
    status VARCHAR(20) CHARACTER SET ascii NOT NULL COMMENT 'SUCCESS | FAILED',
    error_code VARCHAR(60) CHARACTER SET ascii NULL COMMENT 'Mã lỗi AI_PROVIDER_* khi FAILED, NULL khi SUCCESS',
    prompt_tokens INT NOT NULL DEFAULT 0 COMMENT 'Số token đầu vào provider báo; 0 khi lỗi hoặc provider không báo',
    completion_tokens INT NOT NULL DEFAULT 0 COMMENT 'Số token đầu ra provider báo; 0 khi lỗi hoặc provider không báo',
    tool_call_count INT NOT NULL DEFAULT 0 COMMENT 'Số tool call model yêu cầu trong lượt này',
    latency_ms BIGINT NOT NULL COMMENT 'Thời gian gọi provider tính bằng mili giây',
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT 'Thời điểm gọi, UTC',
    CONSTRAINT pk_ai_invocations PRIMARY KEY (id),
    CONSTRAINT chk_ai_invocations_status CHECK (status IN ('SUCCESS', 'FAILED')),
    INDEX ix_ai_invocations_task_name_created_at (task_name, created_at DESC),
    INDEX ix_ai_invocations_created_at (created_at DESC)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
