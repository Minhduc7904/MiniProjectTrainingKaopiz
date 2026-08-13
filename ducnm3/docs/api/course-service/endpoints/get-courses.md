# Liệt kê khóa học

## Tiêu chuẩn phản hồi

Phản hồi JSON thành công sử dụng cấu trúc bao dùng chung trong [`../shared/response-format.md`](../../shared/response-format.md). JSON cụ thể bên dưới là giá trị của `data`; thêm `meta` cho `traceId` và thông tin phân trang. Các điểm cuối truyền CSV và dữ liệu nhị phân theo luồng là ngoại lệ.

`GET /api/courses?status=PUBLISHED&page=1&pageSize=20`

Dữ liệu phản hồi thành công `200 OK`:

```json
{"items":[{"id":"course-uuid","name":"Backend Fundamentals","status":"PUBLISHED"}],"page":1,"pageSize":20,"total":1}
```

- Kiểm tra hợp lệ: `page >= 1`; `pageSize` trong khoảng 1–100; `status` là giá trị liệt kê hợp lệ và không bắt buộc.
- Trạng thái: `400 VALIDATION_ERROR`, `401 UNAUTHENTICATED`.
- Phải ghi tài liệu về thứ tự sắp xếp khi bổ sung phần triển khai.
