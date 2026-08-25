-- API: POST /api/student/courses/{courseId}/enrollments | EnrollCourseHandler
SELECT c.id, c.status FROM courses c WHERE c.id = @courseId LIMIT 2;
SELECT e.id, e.course_id, e.student_id, e.enrolled_at FROM enrollments e WHERE e.course_id = @courseId AND e.student_id = @studentId LIMIT 2;
INSERT INTO enrollments (id, course_id, student_id, enrolled_at) VALUES (@enrollmentId, @courseId, @studentId, @now);
