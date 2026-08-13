# Lấy chi tiết khóa học

## Tiêu chuẩn phản hồi

Phản hồi JSON thành công sử dụng cấu trúc bao dùng chung trong [`../shared/response-format.md`](../../shared/response-format.md). JSON cụ thể bên dưới là giá trị của `data`; thêm `meta` cho `traceId` và thông tin phân trang. Các điểm cuối truyền CSV và dữ liệu nhị phân theo luồng là ngoại lệ.

`GET /api/courses/{courseId}/details`

Dữ liệu phản hồi thành công `200 OK`:

```json
{"id":"course-uuid","name":"Backend Fundamentals","lessons":[{"id":"lesson-uuid","title":"Introduction","displayOrder":1}]}
```

- Trạng thái: `401 UNAUTHENTICATED`, `403 COURSE_ACCESS_DENIED`, `404 COURSE_NOT_FOUND`.
- Phần triển khai phải tránh truy vấn N+1; ghi số lượng truy vấn trong tài liệu đo hiệu năng.
