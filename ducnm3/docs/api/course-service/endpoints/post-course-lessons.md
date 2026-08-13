# Tạo bài học

## Tiêu chuẩn phản hồi

Phản hồi JSON thành công sử dụng cấu trúc bao dùng chung trong [`../shared/response-format.md`](../../shared/response-format.md). JSON cụ thể bên dưới là giá trị của `data`; thêm `meta` cho `traceId` và thông tin phân trang. Các điểm cuối truyền CSV và dữ liệu nhị phân theo luồng là ngoại lệ.

`POST /api/courses/{courseId}/lessons`

Yêu cầu:

```json
{"title":"Introduction","displayOrder":1,"contentMarkdown":"## Start here"}
```

Dữ liệu phản hồi thành công `201 Created`:

```json
{"id":"lesson-uuid","courseId":"course-uuid","displayOrder":1}
```

- Kiểm tra hợp lệ: khóa học tồn tại; `title` là bắt buộc; `displayOrder >= 1`; Markdown an toàn.
- Trạng thái: `400 VALIDATION_ERROR`, `401 UNAUTHENTICATED`, `403 FORBIDDEN`, `404 COURSE_NOT_FOUND`, `409 LESSON_ORDER_CONFLICT`.
- Tác động phụ: tạo bản ghi trong `lessons`.
