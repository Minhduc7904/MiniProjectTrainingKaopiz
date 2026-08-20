# Cập nhật Course

Business flow: [`put-course.md`](../../../business-flows/course-learning/put-course.md).

`PUT /course/api/courses/{courseId}` cập nhật các field xuất hiện trong JSON. `name`, `status` và `descriptionMarkdown` đều tùy chọn; field vắng giữ nguyên. `descriptionMarkdown: null` hoặc chuỗi rỗng xóa nội dung và worker gỡ media usage `COURSE_DESCRIPTION` bất đồng bộ.

Thành công trả `200` response envelope với Course mới. Request không có field, dữ liệu không hợp lệ trả `400 VALIDATION_FAILED`; Course không có trả `404 COURSE_NOT_FOUND`.

```json
{"descriptionMarkdown":"![Ảnh](/media/api/media/11111111-1111-1111-1111-111111111111/content)"}
```
