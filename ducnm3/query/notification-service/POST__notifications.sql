-- API: POST /api/notifications | CreateAsync then SaveChangesAsync
INSERT INTO notifications (id, recipient_student_id, title, body_markdown, source_type, status, created_by, created_at) VALUES (@notificationId, @studentId, @title, @bodyMarkdown, 'DIRECT', 'UNREAD', @createdBy, @now);
