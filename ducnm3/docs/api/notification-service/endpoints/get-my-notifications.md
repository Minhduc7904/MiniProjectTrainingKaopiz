# Liệt kê hộp thư của tôi

## Tiêu chuẩn phản hồi

Phản hồi JSON thành công sử dụng cấu trúc bao dùng chung trong [`../shared/response-format.md`](../../shared/response-format.md). JSON cụ thể bên dưới là giá trị của `data`; thêm `meta` cho `traceId` và thông tin phân trang. Các điểm cuối truyền CSV và dữ liệu nhị phân theo luồng là ngoại lệ.

`GET /api/notifications/me?status=UNREAD&cursor=opaque-token&limit=20`

Dữ liệu phản hồi thành công `200 OK`:

```json
{"items":[{"id":"notification-uuid","title":"Course update","bodyMarkdown":"New material is available.","status":"UNREAD","createdAt":"2026-08-12T06:00:00Z"}],"nextCursor":"opaque-token"}
```

- Kiểm tra hợp lệ: `status` không bắt buộc, nhận `UNREAD|READ`; `limit` trong khoảng 1–100; `cursor` là giá trị không trong suốt.
- Trạng thái: `400 VALIDATION_ERROR`, `401 UNAUTHENTICATED`.
- Tác động phụ: không có; người nhận luôn được xác định từ danh tính.
