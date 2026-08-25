-- API: GET /api/notification-batches/{batchId} | GetByIdAsync
SELECT b.* FROM notification_batches b WHERE b.id = @batchId LIMIT 2;
