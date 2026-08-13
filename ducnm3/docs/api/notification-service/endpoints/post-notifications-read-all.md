# Đánh dấu tất cả thông báo đã đọc

## Tiêu chuẩn phản hồi

Phản hồi JSON thành công sử dụng cấu trúc bao dùng chung trong [`../shared/response-format.md`](../../shared/response-format.md). JSON cụ thể bên dưới là giá trị của `data`; thêm `meta` cho `traceId` và thông tin phân trang. Các điểm cuối truyền CSV và dữ liệu nhị phân theo luồng là ngoại lệ.

`POST /api/notifications/read-all`

Dữ liệu phản hồi thành công `200 OK`:

```json
{"updatedCount":12,"readAt":"2026-08-12T06:05:00Z"}
```

- Kiểm tra hợp lệ: không chấp nhận tham số người nhận.
- Trạng thái: `401 UNAUTHENTICATED`.
- Tác động phụ: chỉ cập nhật các bản ghi `UNREAD` trong hộp thư của bên gọi; thao tác có tính lũy đẳng.
