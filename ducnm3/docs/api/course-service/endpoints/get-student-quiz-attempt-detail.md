# Xem lại một lần làm

Business flow: [`get-student-quiz-attempt-detail.md`](../../../business-flows/course-learning/get-student-quiz-attempt-detail.md).

`GET /course/api/student/enrollments/{courseId}/lessons/{lessonId}/quizzes/{quizId}/attempts/{attemptId}` — Trả đáp án của một lần làm; nếu đã nộp thì kèm kết quả chấm từng câu.

Header bắt buộc: `X-Actor-Type: STUDENT` và `X-Actor-Id` là UUID Student. Student phải đã ghi danh Course (`403 STUDENT_NOT_ENROLLED` nếu chưa) và Quiz phải `PUBLISHED` (`404 QUIZ_NOT_FOUND` nếu không).

## Response

`200 OK`: `data.attempt` (có `answers[]`) và `data.result` (chỉ có khi `SUBMITTED`, cùng cấu trúc với kết quả nộp bài, gồm `correctOptionIds` để đối chiếu). Lần `IN_PROGRESS` trả `result: null`.

Lỗi: `400`, `403 STUDENT_NOT_ENROLLED`, `404 QUIZ_NOT_FOUND`, `404 QUIZ_ATTEMPT_NOT_FOUND` (kể cả lần làm của học viên khác).
