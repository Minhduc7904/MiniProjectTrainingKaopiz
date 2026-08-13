# Lấy học viên đã đăng ký khóa học

## Tiêu chuẩn phản hồi

Phản hồi JSON thành công sử dụng cấu trúc bao dùng chung trong [`../shared/response-format.md`](../../shared/response-format.md). JSON cụ thể bên dưới là giá trị của `data`; thêm `meta` cho `traceId` và thông tin phân trang. Các điểm cuối truyền CSV và dữ liệu nhị phân theo luồng là ngoại lệ.

`GET /api/students/course/{courseId}?cursor=opaque-token&limit=500`

Dữ liệu phản hồi thành công `200 OK`:

```json
{"items":[{"id":"student-uuid","status":"ACTIVE"}],"nextCursor":"opaque-token"}
```

- Điểm cuối nội bộ được Notification Service sử dụng để lấy hàng loạt người nhận.
- Kiểm tra hợp lệ: `courseId` là UUID; `limit` trong khoảng 1–1000; con trỏ là giá trị không trong suốt.
- Trạng thái: `400 VALIDATION_ERROR`, `401 UNAUTHENTICATED`, `403 INTERNAL_SERVICE_ONLY`, `404 COURSE_NOT_FOUND`.
- Phải trả về thứ tự ổn định, xác định được để hỗ trợ bản chụp tác vụ có tính lũy đẳng.
