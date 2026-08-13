# Liệt kê khóa học theo con trỏ

## Tiêu chuẩn phản hồi

Phản hồi JSON thành công sử dụng cấu trúc bao dùng chung trong [`../shared/response-format.md`](../../shared/response-format.md). JSON cụ thể bên dưới là giá trị của `data`; thêm `meta` cho `traceId` và thông tin phân trang. Các điểm cuối truyền CSV và dữ liệu nhị phân theo luồng là ngoại lệ.

`GET /api/courses/cursor?limit=20&cursor=opaque-token`

Dữ liệu phản hồi thành công `200 OK`:

```json
{"items":[{"id":"course-uuid","name":"Backend Fundamentals"}],"nextCursor":"opaque-token"}
```

- Kiểm tra hợp lệ: `limit` trong khoảng 1–100; con trỏ do điểm cuối này tạo và là giá trị không trong suốt.
- Trạng thái: `400 INVALID_CURSOR`, `401 UNAUTHENTICATED`.
- Sử dụng thứ tự sắp xếp ổn định (`created_at`, `id`) để tránh bản ghi bị trùng lặp hoặc bỏ sót.
