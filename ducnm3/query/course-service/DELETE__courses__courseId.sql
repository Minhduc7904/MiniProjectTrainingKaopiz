-- API: DELETE /api/courses/{courseId} | DeleteCourseHandler
SELECT c.id, c.name, c.description_markdown, c.status, c.created_at, c.updated_at FROM courses c WHERE c.id = @courseId LIMIT 2;
SELECT l.id FROM lessons l WHERE l.course_id = @courseId;
-- Media reader is an external HTTP query, not Course ORM.
SELECT c.id, c.name, c.description_markdown, c.status, c.created_at, c.updated_at FROM courses c WHERE c.id = @courseId LIMIT 2;
DELETE FROM courses WHERE id = @courseId;
