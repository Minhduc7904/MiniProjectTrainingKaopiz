-- API: PUT /api/courses/{courseId} | GetAsync then UpdateAsync
SELECT c.id, c.name, c.description_markdown, c.status, c.created_at, c.updated_at FROM courses c WHERE c.id = @courseId LIMIT 2;
SELECT c.id, c.name, c.description_markdown, c.status, c.created_at, c.updated_at FROM courses c WHERE c.id = @courseId LIMIT 2;
UPDATE courses SET name = @name, description_markdown = @descriptionMarkdown, status = @status, updated_at = @now WHERE id = @courseId;
