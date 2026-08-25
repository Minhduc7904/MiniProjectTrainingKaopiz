-- API: GET /api/student/courses/{courseId}/lessons/{lessonId} | IsEnrolledAsync then GetAsync
SELECT EXISTS(SELECT 1 FROM enrollments e WHERE e.course_id = @courseId AND e.student_id = @studentId);
SELECT l.id, l.course_id, l.title, l.content_markdown, l.display_order, l.created_at, l.updated_at FROM lessons l WHERE l.course_id = @courseId AND l.id = @lessonId LIMIT 2;
