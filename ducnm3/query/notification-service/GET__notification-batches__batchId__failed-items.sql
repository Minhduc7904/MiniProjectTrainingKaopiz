-- API: GET /api/notification-batches/{batchId}/failed-items | GetByIdAsync then GetFailedItemsAsync
SELECT b.* FROM notification_batches b WHERE b.id = @batchId LIMIT 2;
SELECT i.id, i.student_id, i.retry_count, i.error_message FROM notification_batch_items i WHERE i.batch_id = @batchId AND i.status = 'FAILED' AND (@afterItemId IS NULL OR i.id > @afterItemId) ORDER BY i.id LIMIT @limitPlusOne;
