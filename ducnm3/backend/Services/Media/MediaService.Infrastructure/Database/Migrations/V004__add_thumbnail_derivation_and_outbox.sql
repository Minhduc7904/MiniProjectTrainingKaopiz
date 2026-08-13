ALTER TABLE media_objects
    ADD COLUMN source_media_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NULL
        COMMENT 'Media gốc của object dẫn xuất; null với file upload gốc'
        AFTER id,
    ADD COLUMN derivation_type VARCHAR(32) CHARACTER SET ascii NULL
        COMMENT 'THUMBNAIL với object dẫn xuất; null với file upload gốc'
        AFTER source_media_id,
    ADD CONSTRAINT fk_media_objects_source_media_id
        FOREIGN KEY (source_media_id) REFERENCES media_objects (id) ON DELETE RESTRICT,
    ADD CONSTRAINT chk_media_objects_derivation
        CHECK (
            (source_media_id IS NULL AND derivation_type IS NULL)
            OR
            (source_media_id IS NOT NULL AND derivation_type = 'THUMBNAIL')
        ),
    ADD INDEX ix_media_objects_source_derivation (source_media_id, derivation_type);

CREATE TABLE media_derivation_jobs (
    id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL
        COMMENT 'UUID định danh operation tạo media dẫn xuất',
    source_media_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL
        COMMENT 'Media gốc cần tạo thumbnail',
    derivative_media_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL
        COMMENT 'Media WebP dẫn xuất được cấp trước',
    derivation_type VARCHAR(32) CHARACTER SET ascii NOT NULL
        COMMENT 'Loại dẫn xuất; hiện chỉ hỗ trợ THUMBNAIL',
    status VARCHAR(20) CHARACTER SET ascii NOT NULL DEFAULT 'QUEUED'
        COMMENT 'QUEUED | PROCESSING | READY | FAILED',
    attempt_count INT UNSIGNED NOT NULL DEFAULT 0
        COMMENT 'Số lần worker đã bắt đầu xử lý',
    last_error VARCHAR(500) NULL
        COMMENT 'Lỗi an toàn của lần xử lý cuối; không trả chi tiết nội bộ',
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)
        COMMENT 'Thời điểm job được tạo, UTC',
    started_at DATETIME(6) NULL
        COMMENT 'Thời điểm lần xử lý gần nhất bắt đầu, UTC',
    completed_at DATETIME(6) NULL
        COMMENT 'Thời điểm job đạt trạng thái terminal, UTC',
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)
        ON UPDATE CURRENT_TIMESTAMP(6)
        COMMENT 'Thời điểm job cập nhật gần nhất, UTC',
    CONSTRAINT pk_media_derivation_jobs PRIMARY KEY (id),
    CONSTRAINT fk_media_derivation_jobs_source
        FOREIGN KEY (source_media_id) REFERENCES media_objects (id) ON DELETE RESTRICT,
    CONSTRAINT fk_media_derivation_jobs_derivative
        FOREIGN KEY (derivative_media_id) REFERENCES media_objects (id) ON DELETE RESTRICT,
    CONSTRAINT uq_media_derivation_jobs_source_type
        UNIQUE (source_media_id, derivation_type),
    CONSTRAINT uq_media_derivation_jobs_derivative
        UNIQUE (derivative_media_id),
    CONSTRAINT chk_media_derivation_jobs_type
        CHECK (derivation_type IN ('THUMBNAIL')),
    CONSTRAINT chk_media_derivation_jobs_status
        CHECK (status IN ('QUEUED', 'PROCESSING', 'READY', 'FAILED')),
    INDEX ix_media_derivation_jobs_status_updated_at (status, updated_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

ALTER TABLE media_usages
    DROP CHECK chk_media_usages_owner_service,
    DROP CHECK chk_media_usages_owner_type,
    DROP CHECK chk_media_usages_usage_type,
    ADD COLUMN active_media_thumbnail_owner_id CHAR(36)
        CHARACTER SET ascii COLLATE ascii_bin
        GENERATED ALWAYS AS (
            CASE
                WHEN owner_service = 'MEDIA'
                    AND owner_type = 'MEDIA_THUMBNAIL'
                    AND usage_type = 'THUMBNAIL'
                    AND deleted_at IS NULL
                THEN owner_id
                ELSE NULL
            END
        ) STORED
        COMMENT 'Media gốc có thumbnail active; đảm bảo tối đa một thumbnail'
        AFTER active_student_avatar_owner_id,
    ADD CONSTRAINT chk_media_usages_owner_service
        CHECK (owner_service IN ('COURSE', 'NOTIFICATION', 'STUDENT', 'MEDIA')),
    ADD CONSTRAINT chk_media_usages_owner_type
        CHECK (owner_type IN (
            'COURSE_THUMBNAIL',
            'COURSE_DESCRIPTION',
            'LESSON_CONTENT',
            'NOTIFICATION_BODY',
            'STUDENT_AVATAR',
            'MEDIA_THUMBNAIL'
        )),
    ADD CONSTRAINT chk_media_usages_usage_type
        CHECK (usage_type IN ('THUMBNAIL', 'EMBED', 'ATTACHMENT', 'AVATAR')),
    ADD CONSTRAINT uq_media_usages_active_media_thumbnail
        UNIQUE (active_media_thumbnail_owner_id);

CREATE TABLE InboxState (
    Id BIGINT NOT NULL AUTO_INCREMENT,
    MessageId CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    ConsumerId CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    LockId CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    RowVersion BINARY(8) NULL,
    Received DATETIME(6) NOT NULL,
    ReceiveCount INT NOT NULL,
    ExpirationTime DATETIME(6) NULL,
    Consumed DATETIME(6) NULL,
    Delivered DATETIME(6) NULL,
    LastSequenceNumber BIGINT NULL,
    CONSTRAINT PK_InboxState PRIMARY KEY (Id),
    CONSTRAINT AK_InboxState_MessageId_ConsumerId UNIQUE (MessageId, ConsumerId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE OutboxState (
    OutboxId CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    LockId CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    RowVersion BINARY(8) NULL,
    Created DATETIME(6) NOT NULL,
    Delivered DATETIME(6) NULL,
    LastSequenceNumber BIGINT NULL,
    CONSTRAINT PK_OutboxState PRIMARY KEY (OutboxId),
    INDEX IX_OutboxState_Created (Created)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE OutboxMessage (
    SequenceNumber BIGINT NOT NULL AUTO_INCREMENT,
    EnqueueTime DATETIME(6) NULL,
    SentTime DATETIME(6) NOT NULL,
    Headers LONGTEXT NULL,
    Properties LONGTEXT NULL,
    InboxMessageId CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NULL,
    InboxConsumerId CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NULL,
    OutboxId CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NULL,
    MessageId CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    ContentType VARCHAR(256) NOT NULL,
    MessageType LONGTEXT NOT NULL,
    Body LONGTEXT NOT NULL,
    ConversationId CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NULL,
    CorrelationId CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NULL,
    InitiatorId CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NULL,
    RequestId CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NULL,
    SourceAddress VARCHAR(256) NULL,
    DestinationAddress VARCHAR(256) NULL,
    ResponseAddress VARCHAR(256) NULL,
    FaultAddress VARCHAR(256) NULL,
    ExpirationTime DATETIME(6) NULL,
    CONSTRAINT PK_OutboxMessage PRIMARY KEY (SequenceNumber),
    INDEX IX_OutboxMessage_EnqueueTime (EnqueueTime),
    INDEX IX_OutboxMessage_ExpirationTime (ExpirationTime),
    INDEX IX_OutboxMessage_InboxMessageId_InboxConsumerId_SequenceNumber (
        InboxMessageId,
        InboxConsumerId,
        SequenceNumber
    ),
    INDEX IX_OutboxMessage_OutboxId_SequenceNumber (OutboxId, SequenceNumber)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
