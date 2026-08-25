-- API: GET /api/students/{studentId} | GetByIdAsync
SELECT s.* FROM students s WHERE s.id = @studentId LIMIT 2;
