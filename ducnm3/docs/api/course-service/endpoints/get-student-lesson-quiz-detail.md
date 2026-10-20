# Đề Quiz (Student)

Business flow: [`get-student-lesson-quiz-detail.md`](../../../business-flows/course-learning/get-student-lesson-quiz-detail.md).

`GET /course/api/student/enrollments/{courseId}/lessons/{lessonId}/quizzes/{quizId}` — Trả đề thi để làm bài; không bao giờ có `isCorrect` hay Markdown nguồn.

Header bắt buộc: `X-Actor-Type: STUDENT` và `X-Actor-Id` là UUID Student. Student phải đã ghi danh Course (`403 STUDENT_NOT_ENROLLED` nếu chưa) và Quiz phải `PUBLISHED` (`404 QUIZ_NOT_FOUND` nếu không).

## Response

`200 OK`, `Cache-Control: no-store`. `data`: `id`, `title`, `descriptionHtml`, `displayOrder`, `questionCount`, `totalPoints`, `questions[]` (`id`, `questionHtml`, `questionType`, `points`, `displayOrder`, `options[]` chỉ gồm `id`, `optionHtml`, `displayOrder`).

## Lỗi

`400`, `403 STUDENT_NOT_ENROLLED`, `404 LESSON_NOT_FOUND`, `404 QUIZ_NOT_FOUND` (không tồn tại hoặc chưa `PUBLISHED`).
