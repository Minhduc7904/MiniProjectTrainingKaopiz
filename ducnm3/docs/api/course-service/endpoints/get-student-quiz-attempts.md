# Các lần làm Quiz

Business flow: [`get-student-quiz-attempts.md`](../../../business-flows/course-learning/get-student-quiz-attempts.md).

`GET /course/api/student/enrollments/{courseId}/lessons/{lessonId}/quizzes/{quizId}/attempts` — Lịch sử các lần làm của chính học viên, mới nhất trước.

Header bắt buộc: `X-Actor-Type: STUDENT` và `X-Actor-Id` là UUID Student. Student phải đã ghi danh Course (`403 STUDENT_NOT_ENROLLED` nếu chưa) và Quiz phải `PUBLISHED` (`404 QUIZ_NOT_FOUND` nếu không).

## Response

`200 OK`, `data[]`: `id`, `status` (`IN_PROGRESS`/`SUBMITTED`), `startedAtUtc`, `updatedAtUtc`, `submittedAtUtc`, `score`, `maxScore`, `answeredQuestions`. Lỗi: `400`, `403 STUDENT_NOT_ENROLLED`, `404 QUIZ_NOT_FOUND`.
