# Danh sách Quiz của Lesson (Student)

API contract: [`GET Danh sách Quiz của Lesson (Student)`](../../api/course-service/endpoints/get-student-lesson-quizzes.md).

Course Service kiểm tra enrollment và Lesson, đọc Quiz `PUBLISHED` rồi gộp thống kê bài làm của chính học viên trong một query phụ (không N+1).
