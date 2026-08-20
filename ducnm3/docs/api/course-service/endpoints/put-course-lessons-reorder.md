# Đổi thứ tự Lessons

`PUT /course/api/courses/{courseId}/lessons/reorder` nhận toàn bộ `lessonIds` theo thứ tự mới. Danh sách phải chứa đúng tất cả Lesson hiện có của Course, không trùng và không có UUID rỗng. Thành công trả `200` response envelope; danh sách sai trả `400` hoặc `409 LESSON_ORDER_CONFLICT`. Endpoint thực hiện cập nhật thứ tự trong Course Service; không thay đổi media usage.
