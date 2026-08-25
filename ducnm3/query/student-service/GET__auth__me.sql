-- API: GET /api/auth/me | GetActiveStudentAsync -> GetByIdAsync
SELECT s.* FROM students s WHERE s.id = @studentId LIMIT 2;
