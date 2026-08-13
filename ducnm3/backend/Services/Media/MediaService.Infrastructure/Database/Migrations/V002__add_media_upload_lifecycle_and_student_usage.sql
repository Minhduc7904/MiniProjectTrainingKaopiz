ALTER TABLE media_objects
    MODIFY checksum_sha256 CHAR(64) CHARACTER SET ascii COLLATE ascii_bin NULL
        COMMENT 'Hash SHA-256 kiểm tra toàn vẹn; null khi upload chưa READY',
    MODIFY created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)
        COMMENT 'Thời điểm tạo media record, UTC',
    ADD COLUMN uploaded_by_type VARCHAR(32) CHARACTER SET ascii NOT NULL
        DEFAULT 'STUDENT'
        COMMENT 'Actor type thực hiện upload; được Application validate'
        AFTER uploaded_by,
    ADD COLUMN status VARCHAR(20) CHARACTER SET ascii NULL
        COMMENT 'PENDING | READY | FAILED'
        AFTER uploaded_by_type,
    ADD COLUMN failure_reason VARCHAR(500) NULL
        COMMENT 'Lỗi an toàn nội bộ khi upload FAILED; không trả cho client'
        AFTER status,
    ADD COLUMN completed_at DATETIME(6) NULL
        COMMENT 'Thời điểm upload chuyển READY, UTC'
        AFTER failure_reason,
    ADD COLUMN updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)
        ON UPDATE CURRENT_TIMESTAMP(6)
        COMMENT 'Thời điểm media record cập nhật gần nhất, UTC'
        AFTER completed_at;

UPDATE media_objects
SET status = 'READY',
    completed_at = created_at
WHERE status IS NULL;

ALTER TABLE media_objects
    MODIFY status VARCHAR(20) CHARACTER SET ascii NOT NULL DEFAULT 'PENDING'
        COMMENT 'PENDING | READY | FAILED',
    ADD CONSTRAINT chk_media_objects_status
        CHECK (status IN ('PENDING', 'READY', 'FAILED')),
    ADD INDEX ix_media_objects_status_created_at (status, created_at);

ALTER TABLE media_usages
    DROP CHECK chk_media_usages_owner_service,
    DROP CHECK chk_media_usages_owner_type,
    DROP CHECK chk_media_usages_usage_type,
    ADD COLUMN created_by_type VARCHAR(32) CHARACTER SET ascii NOT NULL
        DEFAULT 'STUDENT'
        COMMENT 'Actor type tạo usage; được Application validate'
        AFTER created_by,
    ADD COLUMN active_student_avatar_owner_id CHAR(36)
        CHARACTER SET ascii COLLATE ascii_bin
        GENERATED ALWAYS AS (
            CASE
                WHEN owner_service = 'STUDENT'
                    AND owner_type = 'STUDENT_AVATAR'
                    AND usage_type = 'AVATAR'
                    AND deleted_at IS NULL
                THEN owner_id
                ELSE NULL
            END
        ) STORED
        COMMENT 'Student có avatar active; đảm bảo tối đa một avatar'
        AFTER active_course_thumbnail_owner_id,
    ADD CONSTRAINT chk_media_usages_owner_service
        CHECK (owner_service IN ('COURSE', 'NOTIFICATION', 'STUDENT')),
    ADD CONSTRAINT chk_media_usages_owner_type
        CHECK (owner_type IN (
            'COURSE_THUMBNAIL',
            'COURSE_DESCRIPTION',
            'LESSON_CONTENT',
            'NOTIFICATION_BODY',
            'STUDENT_AVATAR'
        )),
    ADD CONSTRAINT chk_media_usages_usage_type
        CHECK (usage_type IN ('THUMBNAIL', 'EMBED', 'ATTACHMENT', 'AVATAR')),
    ADD CONSTRAINT uq_media_usages_active_student_avatar
        UNIQUE (active_student_avatar_owner_id);
