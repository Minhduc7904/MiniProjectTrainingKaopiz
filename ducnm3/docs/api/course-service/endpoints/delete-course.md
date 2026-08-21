# Xóa Course

Business flow: [`delete-course.md`](../../../business-flows/course-learning/delete-course.md).

`DELETE /course/api/courses/{courseId}` hard-delete Course. Database cascade xóa Lesson, enrollment và lesson progress thuộc Course. Thành công trả `202 Accepted` với body rỗng vì Media Worker sẽ dọn usage bất đồng bộ.

## Xác thực và yêu cầu

- Bắt buộc `X-Actor-Id` là UUID Admin hợp lệ.
- `courseId` phải là UUID khác rỗng.
- Course phải tồn tại; request lặp sau khi đã xóa trả `404 COURSE_NOT_FOUND`.

## Tác động phụ

Trước khi hard-delete, Course Service lấy toàn bộ `mediaUsageId` đang hoạt động của các scope `COURSE_DESCRIPTION`, `COURSE_THUMBNAIL`, `COURSE_GALLERY`, `LESSON_CONTENT` và `LESSON_ATTACHMENT` của Course cùng tất cả Lesson. Sau khi database xóa thành công, service gửi một command batch sang Media Worker. Worker soft-delete usage theo ID; ID không còn hoạt động được coi là đã xử lý. Không dùng transaction phân tán giữa Course và Media Service.

## Phản hồi

```http
202 Accepted
```

Không có response body hoặc endpoint theo dõi job.

## Lỗi

- `400 VALIDATION_FAILED`: thiếu/sai `X-Actor-Id` hoặc `courseId` không hợp lệ.
- `404 COURSE_NOT_FOUND`: Course không tồn tại hoặc đã bị xóa.

## Ví dụ

```bash
curl --request DELETE \
  --header 'X-Actor-Id: <admin-uuid>' \
  --header 'X-Correlation-ID: trace-delete-course-001' \
  'http://localhost:5100/course/api/courses/<course-uuid>'
```
