-- API: GET /api/notification-batches/{batchId}/snapshot-status | GetSnapshotProgressAsync
SELECT b.id, b.status, b.requested_count, b.total_count FROM notification_batches b WHERE b.id = @batchId LIMIT 2;
SELECT COUNT(*) FROM notification_batch_items i WHERE i.batch_id = @batchId;
