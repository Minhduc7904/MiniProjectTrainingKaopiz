-- API: GET /api/student/courses/{courseId}/progress | GetProgressAsync
SELECT EXISTS(SELECT 1 FROM courses c WHERE c.id = @courseId);
SELECT l.id, l.title, l.display_order FROM lessons l WHERE l.course_id = @courseId ORDER BY l.display_order;
-- Skipped when no lesson.
SELECT p.lesson_id FROM lesson_progresses p WHERE p.lesson_id IN (@lessonIds) AND p.student_id = @studentId AND p.progress_percent >= 100 AND p.completed_at IS NOT NULL;
