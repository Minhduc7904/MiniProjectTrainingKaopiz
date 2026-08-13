# Xuất khóa học

## Tiêu chuẩn phản hồi

Phản hồi JSON thành công sử dụng cấu trúc bao dùng chung trong [`../shared/response-format.md`](../../shared/response-format.md). JSON cụ thể bên dưới là giá trị của `data`; thêm `meta` cho `traceId` và thông tin phân trang. Các điểm cuối truyền CSV và dữ liệu nhị phân theo luồng là ngoại lệ.

`GET /api/courses/export?status=PUBLISHED`

Dữ liệu phản hồi thành công `200 OK` với `Content-Type: text/csv`:

```csv
id,name,status
course-uuid,Backend Fundamentals,PUBLISHED
```

- Kiểm tra hợp lệ: `status` không bắt buộc và phải hợp lệ.
- Trạng thái: `400 VALIDATION_ERROR`, `401 UNAUTHENTICATED`, `403 FORBIDDEN`.
- Truyền đầu ra theo luồng; không nạp toàn bộ dữ liệu xuất vào bộ nhớ.
