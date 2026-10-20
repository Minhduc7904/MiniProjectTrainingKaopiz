# Lưu nháp đáp án

Business flow: [`put-student-quiz-attempt-answers.md`](../../../business-flows/course-learning/put-student-quiz-attempt-answers.md).

`PUT /course/api/student/enrollments/{courseId}/lessons/{lessonId}/quizzes/{quizId}/attempts/{attemptId}/answers` — Thay toàn bộ đáp án nháp của lần làm đang diễn ra; không chấm điểm.

Header bắt buộc: `X-Actor-Type: STUDENT` và `X-Actor-Id` là UUID Student. Student phải đã ghi danh Course (`403 STUDENT_NOT_ENROLLED` nếu chưa) và Quiz phải `PUBLISHED` (`404 QUIZ_NOT_FOUND` nếu không).

## Request

```json
{ "answers": [ { "questionId": "…", "optionIds": ["…"] }, { "questionId": "…", "optionIds": [] } ] }
```

Cho phép thiếu câu, câu bỏ trống, lưu không giới hạn số lần. Mỗi câu chỉ xuất hiện một lần; `optionIds` phải thuộc đúng câu đó; câu `SINGLE_CHOICE` chỉ nhận tối đa 1 lựa chọn. Danh sách mới **thay thế hoàn toàn** bản nháp cũ.

## Response

`200 OK` với lần làm (như Start). Lỗi: `400 VALIDATION_FAILED`, `403 STUDENT_NOT_ENROLLED`, `404 QUIZ_NOT_FOUND`, `404 QUIZ_ATTEMPT_NOT_FOUND` (không tồn tại hoặc của học viên khác), `409 QUIZ_ATTEMPT_ALREADY_SUBMITTED`.
