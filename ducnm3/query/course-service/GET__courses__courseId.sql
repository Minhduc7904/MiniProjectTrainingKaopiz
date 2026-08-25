-- API: GET /api/courses/{courseId} | GetWithoutNPlusOneAsync
SELECT c.id, c.name, c.description_markdown, c.status, c.created_at FROM courses c WHERE c.id = @courseId LIMIT 2;
-- Runs only when course exists.
SELECT l.id, l.title, l.content_markdown, l.display_order FROM lessons l WHERE l.course_id = @courseId ORDER BY l.display_order;
-- Runs only when the lesson-id array is non-empty.
SELECT p.lesson_id, p.student_id, p.progress_percent, p.completed_at, p.updated_at FROM lesson_progresses p WHERE p.lesson_id IN (@lessonIds);
