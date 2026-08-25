-- API: GET /api/courses/export-buffered-benchmark | ReadAllExportRowsAsync
SELECT c.id, c.name, c.status, c.created_at FROM courses c
WHERE (@status IS NULL OR c.status = @status)
ORDER BY c.created_at DESC, c.id DESC LIMIT @limit;
