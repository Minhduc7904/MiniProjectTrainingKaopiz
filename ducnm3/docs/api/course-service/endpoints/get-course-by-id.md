# Lấy khóa học

## Tiêu chuẩn phản hồi

Phản hồi JSON thành công sử dụng cấu trúc bao dùng chung trong [`../shared/response-format.md`](../../shared/response-format.md). JSON cụ thể bên dưới là giá trị của `data`; thêm `meta` cho `traceId` và thông tin phân trang. Các điểm cuối truyền CSV và dữ liệu nhị phân theo luồng là ngoại lệ.

`GET /api/courses/{courseId}`

Dữ liệu phản hồi thành công `200 OK`:

```json
{"id":"course-uuid","name":"Backend Fundamentals","descriptionMarkdown":"# Overview","status":"PUBLISHED"}
```

- Kiểm tra hợp lệ: `courseId` là UUID.
- Trạng thái: `400 VALIDATION_ERROR`, `401 UNAUTHENTICATED`, `403 COURSE_ACCESS_DENIED`, `404 COURSE_NOT_FOUND`.
- Học viên chỉ có thể đọc khóa học đã xuất bản mà mình được phép truy cập.
