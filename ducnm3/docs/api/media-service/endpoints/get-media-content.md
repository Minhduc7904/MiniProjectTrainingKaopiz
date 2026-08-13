# Lấy nội dung phương tiện

## Tiêu chuẩn phản hồi

Phản hồi JSON thành công sử dụng cấu trúc bao dùng chung trong [`../shared/response-format.md`](../../shared/response-format.md). JSON cụ thể bên dưới là giá trị của `data`; thêm `meta` cho `traceId` và thông tin phân trang. Các điểm cuối truyền CSV và dữ liệu nhị phân theo luồng là ngoại lệ.

`GET /api/media/{mediaId}/content`

Dữ liệu phản hồi thành công: luồng dữ liệu nhị phân `200 OK` với MIME an toàn đã lưu, hoặc `302 Found` đến một URL được ký trước có thời hạn ngắn.

- Kiểm tra hợp lệ: `mediaId` là UUID và phương tiện đang hoạt động.
- Trạng thái: `400 VALIDATION_ERROR`, `401 UNAUTHENTICATED`, `403 MEDIA_ACCESS_DENIED`, `404 MEDIA_NOT_FOUND`.
- Được sử dụng bởi nội dung nhúng Markdown đã làm sạch; chính sách bộ nhớ đệm và quy tắc phân quyền phải được ghi tài liệu trong quá trình triển khai.
