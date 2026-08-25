-- API: GET /api/student/enrollments | GetEnrollmentsAsync
SELECT COUNT(*) FROM enrollments e WHERE e.student_id = @studentId;
SELECT e.id, e.course_id, c.name, c.status, c.created_at, e.enrolled_at FROM enrollments e INNER JOIN courses c ON c.id = e.course_id WHERE e.student_id = @studentId ORDER BY e.enrolled_at DESC, e.id DESC LIMIT @pageSize OFFSET @offset;
