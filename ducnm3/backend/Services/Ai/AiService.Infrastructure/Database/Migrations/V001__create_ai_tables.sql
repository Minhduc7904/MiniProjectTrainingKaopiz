CREATE TABLE ai_providers (
    id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID định danh AI provider',
    name VARCHAR(100) NOT NULL COMMENT 'Tên provider hiển thị cho admin, không trùng nhau',
    provider_type VARCHAR(20) CHARACTER SET ascii NOT NULL COMMENT 'OPENAI | GEMINI | ANTHROPIC | MISTRAL | CUSTOM',
    base_url VARCHAR(500) NOT NULL COMMENT 'Endpoint OpenAI-compatible mà OpenAI SDK sẽ gọi tới',
    api_key_encrypted TEXT CHARACTER SET ascii NOT NULL COMMENT 'API key đã mã hóa AES-GCM, base64 của nonce, tag và ciphertext',
    api_key_hint VARCHAR(16) CHARACTER SET ascii NOT NULL COMMENT 'Gợi ý che key, chỉ chứa 4 ký tự cuối, không dùng để giải mã',
    status VARCHAR(20) CHARACTER SET ascii NOT NULL COMMENT 'ACTIVE | INACTIVE',
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT 'Thời điểm tạo provider, UTC',
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6) COMMENT 'Thời điểm cập nhật gần nhất, UTC',
    CONSTRAINT pk_ai_providers PRIMARY KEY (id),
    CONSTRAINT uq_ai_providers_name UNIQUE (name),
    CONSTRAINT chk_ai_providers_provider_type CHECK (provider_type IN ('OPENAI', 'GEMINI', 'ANTHROPIC', 'MISTRAL', 'CUSTOM')),
    CONSTRAINT chk_ai_providers_status CHECK (status IN ('ACTIVE', 'INACTIVE')),
    INDEX ix_ai_providers_status_created_at (status, created_at DESC)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE ai_models (
    id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID định danh AI model',
    provider_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID provider sở hữu model',
    model_name VARCHAR(100) NOT NULL COMMENT 'Tên model gửi cho provider, ví dụ gpt-4o',
    display_name VARCHAR(100) NOT NULL COMMENT 'Tên hiển thị cho admin',
    status VARCHAR(20) CHARACTER SET ascii NOT NULL COMMENT 'ACTIVE | INACTIVE',
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT 'Thời điểm tạo model, UTC',
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6) COMMENT 'Thời điểm cập nhật gần nhất, UTC',
    CONSTRAINT pk_ai_models PRIMARY KEY (id),
    CONSTRAINT fk_ai_models_provider_id FOREIGN KEY (provider_id) REFERENCES ai_providers (id) ON DELETE RESTRICT,
    CONSTRAINT uq_ai_models_provider_id_model_name UNIQUE (provider_id, model_name),
    CONSTRAINT chk_ai_models_status CHECK (status IN ('ACTIVE', 'INACTIVE')),
    INDEX ix_ai_models_status_created_at (status, created_at DESC)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE ai_service_bindings (
    id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID định danh binding',
    service_name VARCHAR(50) CHARACTER SET ascii NOT NULL COMMENT 'Tên service được dùng AI, ví dụ course-service; logical reference',
    model_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID AI model được gán cho service',
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT 'Thời điểm tạo binding, UTC',
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6) COMMENT 'Thời điểm cập nhật gần nhất, UTC',
    CONSTRAINT pk_ai_service_bindings PRIMARY KEY (id),
    CONSTRAINT fk_ai_service_bindings_model_id FOREIGN KEY (model_id) REFERENCES ai_models (id) ON DELETE RESTRICT,
    CONSTRAINT uq_ai_service_bindings_service_name UNIQUE (service_name),
    INDEX ix_ai_service_bindings_model_id (model_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
