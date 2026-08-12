CREATE TABLE media_objects (
    id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID định danh media',
    bucket VARCHAR(63) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'Tên bucket MinIO chứa object',
    object_key VARCHAR(1024) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'Khóa object duy nhất trong bucket; không trả trực tiếp cho client',
    media_type VARCHAR(20) CHARACTER SET ascii NOT NULL COMMENT 'IMAGE | VIDEO | DOCUMENT | AUDIO | OTHER',
    content_type VARCHAR(255) CHARACTER SET ascii NOT NULL COMMENT 'MIME type đã xác thực',
    original_file_name VARCHAR(255) NOT NULL COMMENT 'Tên file do người dùng upload, chỉ để hiển thị',
    size_bytes BIGINT UNSIGNED NOT NULL COMMENT 'Kích thước object theo byte',
    checksum_sha256 CHAR(64) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'Hash SHA-256 kiểm tra toàn vẹn',
    uploaded_by CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID user/admin upload media; logical reference',
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT 'Thời điểm upload hoàn tất, UTC',
    deleted_at DATETIME(6) NULL COMMENT 'Soft-delete timestamp; null khi media còn hoạt động',
    CONSTRAINT pk_media_objects PRIMARY KEY (id),
    CONSTRAINT uq_media_objects_bucket_object_key UNIQUE (bucket, object_key),
    CONSTRAINT chk_media_objects_media_type CHECK (media_type IN ('IMAGE', 'VIDEO', 'DOCUMENT', 'AUDIO', 'OTHER')),
    INDEX ix_media_objects_uploaded_by_created_at (uploaded_by, created_at DESC)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE media_usages (
    id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID định danh liên kết usage',
    media_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID media_objects.id trong Media Service database',
    owner_service VARCHAR(20) CHARACTER SET ascii NOT NULL COMMENT 'COURSE | NOTIFICATION',
    owner_type VARCHAR(30) CHARACTER SET ascii NOT NULL COMMENT 'COURSE_THUMBNAIL | COURSE_DESCRIPTION | LESSON_CONTENT | NOTIFICATION_BODY',
    owner_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID owner ở owner_service; logical reference',
    usage_type VARCHAR(20) CHARACTER SET ascii NOT NULL COMMENT 'THUMBNAIL | EMBED | ATTACHMENT',
    display_order INT UNSIGNED NOT NULL DEFAULT 0 COMMENT 'Thứ tự render media trong cùng một owner',
    created_by CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL COMMENT 'UUID user/admin tạo liên kết; logical reference',
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT 'Thời điểm tạo liên kết, UTC',
    deleted_at DATETIME(6) NULL COMMENT 'Soft-delete timestamp; null khi usage còn hiệu lực',
    active_course_thumbnail_owner_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin
        GENERATED ALWAYS AS (
            CASE
                WHEN owner_type = 'COURSE_THUMBNAIL' AND deleted_at IS NULL THEN owner_id
                ELSE NULL
            END
        ) STORED COMMENT 'Owner Course có thumbnail còn hiệu lực; dùng để đảm bảo tối đa một thumbnail',
    CONSTRAINT pk_media_usages PRIMARY KEY (id),
    CONSTRAINT fk_media_usages_media_id FOREIGN KEY (media_id) REFERENCES media_objects (id) ON DELETE RESTRICT,
    CONSTRAINT uq_media_usages_reference UNIQUE (media_id, owner_service, owner_type, owner_id, usage_type),
    CONSTRAINT uq_media_usages_active_course_thumbnail UNIQUE (active_course_thumbnail_owner_id),
    CONSTRAINT chk_media_usages_owner_service CHECK (owner_service IN ('COURSE', 'NOTIFICATION')),
    CONSTRAINT chk_media_usages_owner_type CHECK (owner_type IN ('COURSE_THUMBNAIL', 'COURSE_DESCRIPTION', 'LESSON_CONTENT', 'NOTIFICATION_BODY')),
    CONSTRAINT chk_media_usages_usage_type CHECK (usage_type IN ('THUMBNAIL', 'EMBED', 'ATTACHMENT')),
    INDEX ix_media_usages_owner_display_order (owner_service, owner_type, owner_id, display_order)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
