# Nộp bài và chấm điểm

Business flow: [`post-student-quiz-attempt-submit.md`](../../../business-flows/course-learning/post-student-quiz-attempt-submit.md).

`POST /course/api/student/enrollments/{courseId}/lessons/{lessonId}/quizzes/{quizId}/attempts/{attemptId}/submit` — Ghi đáp án cuối (tùy chọn), chấm điểm và chuyển lần làm sang `SUBMITTED`.

Header bắt buộc: `X-Actor-Type: STUDENT` và `X-Actor-Id` là UUID Student. Student phải đã ghi danh Course (`403 STUDENT_NOT_ENROLLED` nếu chưa) và Quiz phải `PUBLISHED` (`404 QUIZ_NOT_FOUND` nếu không).

## Request

```json
{ "answers": [ { "questionId": "…", "optionIds": ["…"] } ] }
```

`answers` bỏ trống/`null` nghĩa là chấm đáp án nháp đã lưu. Có giá trị thì thay bản nháp bằng danh sách này rồi chấm (cùng luật validate như lưu nháp).

## Chấm điểm

Một câu đạt khi **tập đáp án chọn đúng bằng tập đáp án đúng** (chọn nhiều là “đúng toàn bộ mới có điểm”): được `points` của câu, ngược lại 0; bỏ trống = 0. `score = Σ điểm đạt`, `maxScore = Σ points` (snapshot lúc nộp).

## Response

`200 OK`: `data.attempt` (`status: SUBMITTED`, `score`, `maxScore`, `answers`) và `data.result`: `score`, `maxScore`, `correctCount`, `incorrectCount`, `unansweredCount`, `questions[]` (`questionId`, `answered`, `correct`, `pointsEarned`, `pointsPossible`, `selectedOptionIds`, `correctOptionIds`).

Lỗi: `400`, `403 STUDENT_NOT_ENROLLED`, `404 QUIZ_ATTEMPT_NOT_FOUND`, `409 QUIZ_ATTEMPT_ALREADY_SUBMITTED` (nộp lần hai).
