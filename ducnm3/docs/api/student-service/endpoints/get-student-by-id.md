# Lấy học viên

## Tiêu chuẩn phản hồi

Phản hồi JSON thành công sử dụng cấu trúc bao dùng chung trong [`../shared/response-format.md`](../../shared/response-format.md). JSON cụ thể bên dưới là giá trị của `data`; thêm `meta` cho `traceId` và thông tin phân trang. Các điểm cuối truyền CSV và dữ liệu nhị phân theo luồng là ngoại lệ.

`GET /api/students/{studentId}`

Dữ liệu phản hồi thành công `200 OK`:

```json
{"id":"student-uuid","email":"student@example.com","displayName":"Student One","status":"ACTIVE"}
```

- Kiểm tra hợp lệ: `studentId` là UUID.
- Trạng thái: `400 VALIDATION_ERROR`, `401 UNAUTHENTICATED`, `403 STUDENT_ACCESS_DENIED`, `404 STUDENT_NOT_FOUND`.
