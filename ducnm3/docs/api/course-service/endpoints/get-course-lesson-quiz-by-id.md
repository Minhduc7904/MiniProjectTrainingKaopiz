# Lấy chi tiết Quiz (Admin)

Business flow: [`get-course-lesson-quiz-by-id.md`](../../../business-flows/course-learning/get-course-lesson-quiz-by-id.md).

`GET /course/api/courses/{courseId}/lessons/{lessonId}/quizzes/{quizId}` — Trả Quiz đầy đủ gồm Markdown nguồn, HTML đã render và cờ `isCorrect` của từng lựa chọn.

Header bắt buộc: `X-Actor-Type: ADMIN` và `X-Actor-Id` là UUID hợp lệ. `courseId`, `lessonId`, `quizId` phải là UUID.

## Response

`200 OK`, `Cache-Control: no-store`, `data` có cấu trúc như response tạo Quiz. `hasAttempts` cho biết đã có học viên làm bài (khi đó cây câu hỏi bị khóa, xem [PUT](put-course-lesson-quiz.md)).

## Lỗi

`400 VALIDATION_FAILED`, `403 FORBIDDEN`, `404 LESSON_NOT_FOUND`, `404 QUIZ_NOT_FOUND`.
