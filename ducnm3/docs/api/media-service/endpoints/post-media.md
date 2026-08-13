# Tải phương tiện lên

## Tiêu chuẩn phản hồi

Phản hồi JSON thành công sử dụng cấu trúc bao dùng chung trong [`../shared/response-format.md`](../../shared/response-format.md). JSON cụ thể bên dưới là giá trị của `data`; thêm `meta` cho `traceId` và thông tin phân trang. Các điểm cuối truyền CSV và dữ liệu nhị phân theo luồng là ngoại lệ.

`POST /api/media` (`multipart/form-data`)

Các trường của yêu cầu: `file`, `mediaType`.

Dữ liệu phản hồi thành công `201 Created`:

```json
{"id":"media-uuid","mediaType":"IMAGE","contentType":"image/webp","sizeBytes":24576,"contentUrl":"/api/media/media-uuid/content"}
```

- Kiểm tra hợp lệ: tệp là bắt buộc; MIME được phép phải khớp với `mediaType`; kích thước nằm trong giới hạn đã cấu hình; tổng kiểm được tính ở phía máy chủ.
- Trạng thái: `400 INVALID_MEDIA`, `401 UNAUTHENTICATED`, `413 MEDIA_TOO_LARGE`, `415 UNSUPPORTED_MEDIA_TYPE`.
- Tác động phụ: ghi dữ liệu nhị phân vào MinIO và siêu dữ liệu vào `media_objects`.
