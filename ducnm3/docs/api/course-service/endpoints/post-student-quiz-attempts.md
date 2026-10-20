# Bắt đầu hoặc tiếp tục làm Quiz

Business flow: [`post-student-quiz-attempts.md`](../../../business-flows/course-learning/post-student-quiz-attempts.md).

`POST /course/api/student/enrollments/{courseId}/lessons/{lessonId}/quizzes/{quizId}/attempts` — Tạo lần làm mới, hoặc trả lại chính lần đang làm dở kèm đáp án đã lưu.

Header bắt buộc: `X-Actor-Type: STUDENT` và `X-Actor-Id` là UUID Student. Student phải đã ghi danh Course (`403 STUDENT_NOT_ENROLLED` nếu chưa) và Quiz phải `PUBLISHED` (`404 QUIZ_NOT_FOUND` nếu không).

Không có body. Quiz không giới hạn thời gian và không có hạn hết hạn bài làm.

## Response

- `201 Created` khi tạo lần làm mới (chưa có lần `IN_PROGRESS`, bao gồm làm lại sau khi đã nộp).
- `200 OK` khi đã có lần `IN_PROGRESS`: trả đúng lần đó để tiếp tục (resume).

Cả hai kèm `Location` tới lần làm và `data`: `id`, `quizId`, `status`, `startedAtUtc`, `updatedAtUtc`, `submittedAtUtc`, `score`, `maxScore`, `answeredQuestions`, `answers[]` (`questionId`, `optionIds[]`).

## Lỗi

`400`, `403 STUDENT_NOT_ENROLLED`, `404 QUIZ_NOT_FOUND`.
