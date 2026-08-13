# Liệt kê học viên

## Tiêu chuẩn phản hồi

Phản hồi JSON thành công sử dụng cấu trúc bao dùng chung trong [`../shared/response-format.md`](../../shared/response-format.md). JSON cụ thể bên dưới là giá trị của `data`; thêm `meta` cho `traceId` và thông tin phân trang. Các điểm cuối truyền CSV và dữ liệu nhị phân theo luồng là ngoại lệ.

`GET /api/students?status=ACTIVE&page=1&pageSize=20`

Dữ liệu phản hồi thành công `200 OK`:

```json
{"items":[{"id":"student-uuid","email":"student@example.com","displayName":"Student One","status":"ACTIVE"}],"page":1,"pageSize":20,"total":1}
```

- Kiểm tra hợp lệ: `status` là giá trị liệt kê hợp lệ và không bắt buộc; `page >= 1`; `pageSize` trong khoảng 1–100.
- Trạng thái: `400 VALIDATION_ERROR`, `401 UNAUTHENTICATED`, `403 FORBIDDEN`.
