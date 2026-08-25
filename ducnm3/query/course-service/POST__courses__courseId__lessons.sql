-- API: POST /api/courses/{courseId}/lessons | CreateAsync
SELECT EXISTS(SELECT 1 FROM courses c WHERE c.id = @courseId);
-- Only when displayOrder is null.
SELECT MAX(l.display_order) FROM lessons l WHERE l.course_id = @courseId;
INSERT INTO lessons (id, course_id, title, content_markdown, display_order, created_at, updated_at)
VALUES (@lessonId, @courseId, @title, @contentMarkdown, @displayOrder, @now, @now);
