-- API: GET /api/notification-batches/{batchId}/delivery-status | GetByIdAsync
SELECT b.* FROM notification_batches b WHERE b.id = @batchId LIMIT 2;
