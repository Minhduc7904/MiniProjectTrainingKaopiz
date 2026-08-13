# Tạo thông tin sử dụng phương tiện

## Tiêu chuẩn phản hồi

Phản hồi JSON thành công sử dụng cấu trúc bao dùng chung trong [`../shared/response-format.md`](../../shared/response-format.md). JSON cụ thể bên dưới là giá trị của `data`; thêm `meta` cho `traceId` và thông tin phân trang. Các điểm cuối truyền CSV và dữ liệu nhị phân theo luồng là ngoại lệ.

`POST /api/media/usages`

Yêu cầu:

```json
{"mediaId":"media-uuid","ownerService":"COURSE","ownerType":"COURSE_THUMBNAIL","ownerId":"course-uuid","usageType":"THUMBNAIL"}
```

Dữ liệu phản hồi thành công `201 Created`:

```json
{"id":"usage-uuid","mediaId":"media-uuid","ownerService":"COURSE","ownerType":"COURSE_THUMBNAIL","ownerId":"course-uuid","usageType":"THUMBNAIL"}
```

- Kiểm tra hợp lệ: tất cả ID đều là UUID; tổ hợp giá trị liệt kê về chủ sở hữu được phép; phương tiện đang hoạt động.
- Trạng thái: `400 VALIDATION_ERROR`, `401 UNAUTHENTICATED`, `403 OWNER_ACCESS_DENIED`, `404 MEDIA_NOT_FOUND`, `409 MEDIA_USAGE_CONFLICT`.
- Với `COURSE_THUMBNAIL`, thay thế thumbnail đang hoạt động trước đó theo cơ chế nguyên tử.
