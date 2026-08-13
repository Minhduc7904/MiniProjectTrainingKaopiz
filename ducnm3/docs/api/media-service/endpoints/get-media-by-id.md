# Lấy siêu dữ liệu của phương tiện

## Tiêu chuẩn phản hồi

Phản hồi JSON thành công sử dụng cấu trúc bao dùng chung trong [`../shared/response-format.md`](../../shared/response-format.md). JSON cụ thể bên dưới là giá trị của `data`; thêm `meta` cho `traceId` và thông tin phân trang. Các điểm cuối truyền CSV và dữ liệu nhị phân theo luồng là ngoại lệ.

`GET /api/media/{mediaId}`

Dữ liệu phản hồi thành công `200 OK`:

```json
{"id":"media-uuid","mediaType":"IMAGE","contentType":"image/webp","sizeBytes":24576,"contentUrl":"/api/media/media-uuid/content"}
```

- Kiểm tra hợp lệ: `mediaId` là UUID.
- Trạng thái: `400 VALIDATION_ERROR`, `401 UNAUTHENTICATED`, `403 MEDIA_ACCESS_DENIED`, `404 MEDIA_NOT_FOUND`.
- Tuyệt đối không trả về `bucket` hoặc `object_key`.
