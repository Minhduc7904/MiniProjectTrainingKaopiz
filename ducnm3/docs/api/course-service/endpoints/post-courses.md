# Tạo khóa học

## Tiêu chuẩn phản hồi

Phản hồi JSON thành công sử dụng cấu trúc bao dùng chung trong [`../shared/response-format.md`](../../shared/response-format.md). JSON cụ thể bên dưới là giá trị của `data`; thêm `meta` cho `traceId` và thông tin phân trang. Các điểm cuối truyền CSV và dữ liệu nhị phân theo luồng là ngoại lệ.

`POST /api/courses`

Yêu cầu:

```json
{"name":"Backend Fundamentals","descriptionMarkdown":"# Overview","status":"DRAFT"}
```

Dữ liệu phản hồi thành công `201 Created`:

```json
{"id":"course-uuid","name":"Backend Fundamentals","status":"DRAFT","createdAt":"2026-08-12T06:00:00Z"}
```

- Kiểm tra hợp lệ: `name` là bắt buộc, dài 3–200 ký tự; `status` là `DRAFT`, `PUBLISHED` hoặc `ARCHIVED`; Markdown không chứa HTML không an toàn.
- Trạng thái: `400 VALIDATION_ERROR`, `401 UNAUTHENTICATED`, `403 FORBIDDEN`, `409 COURSE_NAME_CONFLICT`.
- Tác động phụ: tạo bản ghi trong `courses`. Phương tiện được đăng ký riêng thông qua Media Service.
