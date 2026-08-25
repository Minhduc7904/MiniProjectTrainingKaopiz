-- API: POST /api/notification-batches | CreateAsync then SaveChangesAsync
INSERT INTO notification_batches (id, title, body_markdown, target_scope, created_by, status, total_count, batch_size, requested_count, source_batch_id, created_at) VALUES (@batchId, @title, @bodyMarkdown, 'ALL_STUDENTS', @createdBy, 'PENDING', 0, @batchSize, @requestedCount, NULL, @now);
