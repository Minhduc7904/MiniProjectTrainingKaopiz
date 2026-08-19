-- Bổ sung giới hạn recipient, quan hệ retry và index phục vụ quản lý Notification Batch.
ALTER TABLE notification_batches
    ADD COLUMN requested_count INT UNSIGNED NULL
        COMMENT 'Số recipient người dùng yêu cầu; null nghĩa là toàn bộ' AFTER batch_size,
    ADD COLUMN source_batch_id CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NULL
        COMMENT 'Batch nguồn khi retry riêng các recipient FAILED' AFTER requested_count,
    ADD CONSTRAINT ck_notification_batches_requested_count
        CHECK (requested_count IS NULL OR requested_count BETWEEN 1 AND 100000),
    ADD CONSTRAINT fk_notification_batches_source_batch_id
        FOREIGN KEY (source_batch_id) REFERENCES notification_batches(id) ON DELETE RESTRICT,
    ADD CONSTRAINT uq_notification_batches_source_batch_id UNIQUE (source_batch_id);

ALTER TABLE notification_batches
    DROP CHECK chk_notification_batches_target_scope,
    ADD CONSTRAINT chk_notification_batches_target_scope
        CHECK (target_scope IN ('COURSE_ENROLLED', 'STUDENT_IDS', 'ALL_STUDENTS', 'FAILED_RECIPIENTS'));

DROP INDEX ix_notification_batches_status_created_at ON notification_batches;
CREATE INDEX ix_notification_batches_status_created_at_id
    ON notification_batches(status, created_at DESC, id DESC);
CREATE INDEX ix_notification_batches_created_at_id
    ON notification_batches(created_at DESC, id DESC);
