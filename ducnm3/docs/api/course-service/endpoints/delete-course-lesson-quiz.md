# Xóa Quiz

Business flow: [`delete-course-lesson-quiz.md`](../../../business-flows/course-learning/delete-course-lesson-quiz.md).

`DELETE /course/api/courses/{courseId}/lessons/{lessonId}/quizzes/{quizId}` — Hard-delete Quiz cùng câu hỏi, lựa chọn và toàn bộ bài làm (FK `ON DELETE CASCADE`).

Header bắt buộc: `X-Actor-Type: ADMIN` và `X-Actor-Id` là UUID hợp lệ. `courseId`, `lessonId`, `quizId` phải là UUID.

## Response

`202 Accepted`, body rỗng (cùng quy ước với xóa Lesson/Course vì dọn media là bất đồng bộ). Lặp lại sau khi xóa trả `404 QUIZ_NOT_FOUND`.

## Lỗi

`400 VALIDATION_FAILED`, `403 FORBIDDEN`, `404 LESSON_NOT_FOUND`, `404 QUIZ_NOT_FOUND`.
