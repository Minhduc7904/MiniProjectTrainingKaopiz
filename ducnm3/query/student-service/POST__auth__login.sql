-- API: POST /api/auth/login | GetByIdAsync
SELECT s.* FROM students s WHERE s.id = @studentId LIMIT 2;
