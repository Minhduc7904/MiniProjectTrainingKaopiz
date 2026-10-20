# Liệt kê Quiz của Lesson

Business flow: [`get-course-lesson-quizzes.md`](../../../business-flows/course-learning/get-course-lesson-quizzes.md).

`GET /course/api/courses/{courseId}/lessons/{lessonId}/quizzes` — Trả danh sách Quiz (mọi trạng thái) của Lesson cho Admin, sắp theo `displayOrder`.

Header bắt buộc: `X-Actor-Type: ADMIN` và `X-Actor-Id` là UUID hợp lệ. `courseId`, `lessonId`, `quizId` phải là UUID.

## Response

`200 OK`, `Cache-Control: no-store`, `data` là mảng tóm tắt: `id`, `lessonId`, `title`, `descriptionMarkdown`, `descriptionHtml`, `displayOrder`, `status`, `questionCount`, `totalPoints`, `hasAttempts`, `createdAtUtc`, `updatedAtUtc`. Không phân trang vì một Lesson chỉ có số ít Quiz.

## Lỗi

`400 VALIDATION_FAILED` (UUID sai), `403 FORBIDDEN`, `404 LESSON_NOT_FOUND`.
