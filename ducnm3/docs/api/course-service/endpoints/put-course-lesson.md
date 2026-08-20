# Cập nhật Lesson

Business flow: [`put-course-lesson.md`](../../../business-flows/course-learning/put-course-lesson.md).

`PUT /course/api/courses/{courseId}/lessons/{lessonId}` cập nhật partial `title` và `contentMarkdown`. Field vắng giữ nguyên; `contentMarkdown: null` hoặc chuỗi rỗng xóa nội dung và worker gỡ media usage `LESSON_CONTENT` bất đồng bộ. Thứ tự chỉ đổi qua endpoint reorder riêng. Thành công trả `200` response envelope; request rỗng/không hợp lệ trả `400`, Lesson không tồn tại trả `404`.
