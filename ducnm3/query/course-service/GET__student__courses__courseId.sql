-- API: GET /api/student/courses/{courseId} | GetDetailAsync
SELECT c.id, c.name, c.description_markdown, c.status, c.created_at FROM courses c WHERE c.id = @courseId LIMIT 2;
SELECT l.id, l.title, l.display_order, COALESCE(p.progress_percent, 0), p.completed_at FROM lessons l LEFT JOIN lesson_progresses p ON p.lesson_id = l.id AND p.student_id = @studentId WHERE l.course_id = @courseId ORDER BY l.display_order;
