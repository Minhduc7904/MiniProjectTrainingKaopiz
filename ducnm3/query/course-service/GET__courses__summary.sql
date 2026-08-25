-- API: GET /api/courses/summary | CountAsync has two awaited ORM queries.
SELECT COUNT(*) FROM courses;
SELECT COUNT(*) FROM lessons;
