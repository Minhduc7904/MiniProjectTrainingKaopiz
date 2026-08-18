ALTER TABLE media_objects
    ADD COLUMN is_draft TINYINT(1) NOT NULL DEFAULT 1
        COMMENT 'Media chưa được gắn với usage active'
        AFTER status,
    ADD COLUMN drafted_at DATETIME(6) NULL
        COMMENT 'Thời điểm media bắt đầu ở trạng thái draft, UTC'
        AFTER completed_at;

UPDATE media_objects AS media
SET media.is_draft = CASE
        WHEN EXISTS (
            SELECT 1
            FROM media_usages AS media_usage
            WHERE media_usage.media_id = media.id
              AND media_usage.deleted_at IS NULL
        ) THEN 0
        ELSE 1
    END,
    media.drafted_at = CASE
        WHEN EXISTS (
            SELECT 1
            FROM media_usages AS media_usage
            WHERE media_usage.media_id = media.id
              AND media_usage.deleted_at IS NULL
        ) THEN NULL
        ELSE COALESCE(media.completed_at, media.created_at)
    END;

CREATE INDEX ix_media_objects_draft_cleanup
    ON media_objects (is_draft, status, deleted_at, drafted_at);
