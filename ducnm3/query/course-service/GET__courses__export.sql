-- API: GET /api/courses/export | ReadExportChunkAsync; this SELECT runs once per 500-row chunk.
SELECT c.id, c.name, c.status, c.created_at FROM courses c
WHERE (@status IS NULL OR c.status = @status)
  AND (@lastCreatedAt IS NULL OR c.created_at < @lastCreatedAt OR (c.created_at = @lastCreatedAt AND c.id < @lastId))
ORDER BY c.created_at DESC, c.id DESC LIMIT @chunkSize;
