-- API: GET /api/student/courses | GetCatalogAsync
SELECT COUNT(*) FROM courses c WHERE c.status = 'PUBLISHED'
AND NOT EXISTS (SELECT 1 FROM enrollments e WHERE e.course_id = c.id AND e.student_id = @studentId)
AND (@search IS NULL OR c.name LIKE CONCAT('%', @search, '%'));
SELECT c.id, c.name, c.status, c.created_at FROM courses c WHERE c.status = 'PUBLISHED'
AND NOT EXISTS (SELECT 1 FROM enrollments e WHERE e.course_id = c.id AND e.student_id = @studentId)
AND (@search IS NULL OR c.name LIKE CONCAT('%', @search, '%')) ORDER BY c.created_at DESC, c.id DESC LIMIT @pageSize OFFSET @offset;
