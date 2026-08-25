-- API: POST /api/courses | CreateCourseHandler -> CreateAsync -> SaveChangesAsync
INSERT INTO courses (id, name, description_markdown, status, created_at, updated_at)
VALUES (@courseId, @name, @descriptionMarkdown, 'DRAFT', @now, @now);
