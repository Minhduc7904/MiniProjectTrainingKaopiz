# Đánh dấu thông báo đã đọc

## Tiêu chuẩn phản hồi

Phản hồi JSON thành công sử dụng cấu trúc bao dùng chung trong [`../shared/response-format.md`](../../shared/response-format.md). JSON cụ thể bên dưới là giá trị của `data`; thêm `meta` cho `traceId` và thông tin phân trang. Các điểm cuối truyền CSV và dữ liệu nhị phân theo luồng là ngoại lệ.

`PATCH /api/notifications/{notificationId}/read`

Dữ liệu phản hồi thành công `200 OK`:

```json
{"id":"notification-uuid","status":"READ","readAt":"2026-08-12T06:05:00Z"}
```

- Kiểm tra hợp lệ: ID thông báo là UUID.
- Trạng thái: `400 VALIDATION_ERROR`, `401 UNAUTHENTICATED`, `403 NOTIFICATION_ACCESS_DENIED`, `404 NOTIFICATION_NOT_FOUND`.
- Lũy đẳng: các lần gọi lặp lại trả về cùng trạng thái đã đọc mà không thay đổi `read_at` ban đầu.
