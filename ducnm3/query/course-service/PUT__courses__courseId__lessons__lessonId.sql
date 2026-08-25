-- API: PUT /api/courses/{courseId}/lessons/{lessonId} | GetAsync then UpdateAsync
SELECT l.id, l.course_id, l.title, l.content_markdown, l.display_order, l.created_at, l.updated_at FROM lessons l WHERE l.course_id = @courseId AND l.id = @lessonId LIMIT 2;
SELECT l.id, l.course_id, l.title, l.content_markdown, l.display_order, l.created_at, l.updated_at FROM lessons l WHERE l.course_id = @courseId AND l.id = @lessonId LIMIT 2;
UPDATE lessons SET title = @title, content_markdown = @contentMarkdown, updated_at = @now WHERE course_id = @courseId AND id = @lessonId;
