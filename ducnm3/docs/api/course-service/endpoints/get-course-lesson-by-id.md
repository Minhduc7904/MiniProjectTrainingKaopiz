# Lấy chi tiết Lesson

Business flow: [`get-course-lesson-by-id.md`](../../../business-flows/course-learning/get-course-lesson-by-id.md).

`GET /course/api/courses/{courseId}/lessons/{lessonId}` chỉ đọc Lesson thuộc Course, không có request body và trả `200` response envelope gồm `title`, `contentMarkdown`, `displayOrder`, timestamps và `attachments`. Mỗi attachment có URL nội dung, thumbnail (nếu Media đã sinh), loại media, content type và tên file. ID không hợp lệ trả `400 VALIDATION_FAILED`; Lesson không thuộc Course hoặc không tồn tại trả `404 LESSON_NOT_FOUND`. Cache policy là `Cache-Control: no-store`; endpoint safe, idempotent và không tạo side effect.
