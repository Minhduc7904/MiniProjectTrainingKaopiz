-- API: GET /api/students | GetListAsync
SELECT COUNT(*) FROM students s WHERE (@status IS NULL OR s.status = @status) AND (@search IS NULL OR s.display_name LIKE CONCAT('%', @search, '%') OR s.email LIKE CONCAT('%', @search, '%'));
SELECT s.id, s.email, s.display_name, s.status, s.created_at FROM students s WHERE (@status IS NULL OR s.status = @status) AND (@search IS NULL OR s.display_name LIKE CONCAT('%', @search, '%') OR s.email LIKE CONCAT('%', @search, '%')) ORDER BY s.email, s.id LIMIT @pageSize OFFSET @offset;
