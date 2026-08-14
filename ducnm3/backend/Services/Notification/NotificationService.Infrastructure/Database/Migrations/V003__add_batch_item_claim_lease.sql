ALTER TABLE notification_batch_items
    ADD COLUMN lease_token CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NULL
        COMMENT 'UUID token sở hữu claim PROCESSING hiện tại; null khi item chưa được claim hoặc đã hoàn tất',
    ADD COLUMN lease_expires_at DATETIME(6) NULL
        COMMENT 'Thời điểm UTC claim PROCESSING hết hạn để worker khác có thể nhận lại';

CREATE INDEX ix_notification_batch_items_claim
    ON notification_batch_items (batch_id, status, lease_expires_at, id);
