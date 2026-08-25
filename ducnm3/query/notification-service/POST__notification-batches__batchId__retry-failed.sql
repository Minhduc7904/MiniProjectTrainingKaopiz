-- API: POST /api/notification-batches/{batchId}/retry-failed
SELECT b.* FROM notification_batches b WHERE b.id = @sourceBatchId LIMIT 2;
SELECT b.* FROM notification_batches b WHERE b.source_batch_id = @sourceBatchId LIMIT 2;
-- Only when no idempotent child exists.
SELECT b.* FROM notification_batches b WHERE b.id = @sourceBatchId LIMIT 2;
INSERT INTO notification_batches (id, title, body_markdown, target_scope, created_by, status, total_count, batch_size, requested_count, source_batch_id, created_at) VALUES (@childBatchId, @title, @bodyMarkdown, 'FAILED_RECIPIENTS', @createdBy, 'PENDING', 0, @batchSize, @failedCount, @sourceBatchId, @now);
-- DbUpdate duplicate-key recovery branch:
SELECT b.* FROM notification_batches b WHERE b.source_batch_id = @sourceBatchId LIMIT 2;
