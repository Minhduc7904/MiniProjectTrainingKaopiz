-- API: GET /api/notification-batches | ListAsync
SELECT COUNT(*) FROM notification_batches b WHERE (@status IS NULL OR b.status = @status);
SELECT b.* FROM notification_batches b WHERE (@status IS NULL OR b.status = @status) ORDER BY b.created_at DESC, b.id DESC LIMIT @pageSize OFFSET @offset;
