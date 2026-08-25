-- API: DELETE /api/courses/{courseId}/lessons/{lessonId} | GetAsync then DeleteAsync
SELECT l.id, l.course_id, l.title, l.content_markdown, l.display_order, l.created_at, l.updated_at FROM lessons l WHERE l.course_id = @courseId AND l.id = @lessonId LIMIT 2;
-- Media reader is external.
SELECT l.id, l.course_id, l.title, l.content_markdown, l.display_order, l.created_at, l.updated_at FROM lessons l WHERE l.course_id = @courseId AND l.id = @lessonId LIMIT 2;
DELETE FROM lessons WHERE course_id = @courseId AND id = @lessonId;
