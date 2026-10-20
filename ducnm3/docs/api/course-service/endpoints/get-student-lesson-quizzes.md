# Danh sách Quiz của Lesson (Student)

Business flow: [`get-student-lesson-quizzes.md`](../../../business-flows/course-learning/get-student-lesson-quizzes.md).

`GET /course/api/student/enrollments/{courseId}/lessons/{lessonId}/quizzes` — Trả các Quiz `PUBLISHED` của Lesson kèm điểm cao nhất và bài làm dở của học viên.

Header bắt buộc: `X-Actor-Type: STUDENT` và `X-Actor-Id` là UUID Student. Student phải đã ghi danh Course (`403 STUDENT_NOT_ENROLLED` nếu chưa) và Quiz phải `PUBLISHED` (`404 QUIZ_NOT_FOUND` nếu không).

## Response

`200 OK`, `Cache-Control: no-store`, `data[]`: `id`, `title`, `descriptionHtml`, `displayOrder`, `questionCount`, `totalPoints`, `bestScore`, `bestMaxScore`, `submittedAttempts`, `inProgressAttemptId`. Không có Markdown nguồn.

## Lỗi

`400 VALIDATION_FAILED`, `403 FORBIDDEN` / `403 STUDENT_NOT_ENROLLED`, `404 LESSON_NOT_FOUND`.
