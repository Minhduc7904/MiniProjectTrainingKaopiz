-- API: GET /api/notifications/{notificationId} | GetByIdAsync
SELECT n.id, n.recipient_student_id, n.title, n.body_markdown, n.source_type, n.status, n.created_by, n.created_at, n.read_at FROM notifications n WHERE n.id = @notificationId LIMIT 2;
