-- API: POST /api/student/courses/{courseId}/lessons/{lessonId}/complete | CompleteLessonProgressHandler
SELECT c.id, c.status FROM courses c WHERE c.id = @courseId LIMIT 2;
SELECT EXISTS(SELECT 1 FROM lessons l WHERE l.id = @lessonId AND l.course_id = @courseId);
SELECT e.id, e.course_id, e.student_id, e.enrolled_at FROM enrollments e WHERE e.course_id = @courseId AND e.student_id = @studentId LIMIT 2;
SELECT p.id, p.lesson_id, p.student_id, p.progress_percent, p.completed_at, p.updated_at FROM lesson_progresses p WHERE p.lesson_id = @lessonId AND p.student_id = @studentId LIMIT 2;
-- New item branch:
INSERT INTO lesson_progresses (id, lesson_id, student_id, progress_percent, completed_at, updated_at) VALUES (@progressId, @lessonId, @studentId, 100, @now, @now);
-- Existing incomplete item branch:
UPDATE lesson_progresses SET progress_percent = 100, completed_at = COALESCE(completed_at, @now), updated_at = @now WHERE lesson_id = @lessonId AND student_id = @studentId;
