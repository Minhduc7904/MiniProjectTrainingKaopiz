ALTER TABLE media_usages
    DROP INDEX uq_media_usages_reference,
    ADD COLUMN active_reference_guard TINYINT
        GENERATED ALWAYS AS (
            CASE WHEN deleted_at IS NULL THEN 1 ELSE NULL END
        ) STORED
        COMMENT 'Chỉ áp dụng unique reference cho usage active'
        AFTER active_student_avatar_owner_id,
    ADD CONSTRAINT uq_media_usages_active_reference
        UNIQUE (
            media_id,
            owner_service,
            owner_type,
            owner_id,
            usage_type,
            active_reference_guard
        );
