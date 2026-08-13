# Liệt kê thông tin sử dụng phương tiện

## Tiêu chuẩn phản hồi

Phản hồi JSON thành công sử dụng cấu trúc bao dùng chung trong [`../shared/response-format.md`](../../shared/response-format.md). JSON cụ thể bên dưới là giá trị của `data`; thêm `meta` cho `traceId` và thông tin phân trang. Các điểm cuối truyền CSV và dữ liệu nhị phân theo luồng là ngoại lệ.

`GET /api/media/usages?ownerService=COURSE&ownerType=COURSE_THUMBNAIL&ownerId=course-uuid`

Dữ liệu phản hồi thành công `200 OK`:

```json
{"items":[{"id":"usage-uuid","mediaId":"media-uuid","usageType":"THUMBNAIL","contentUrl":"/api/media/media-uuid/content"}]}
```

- Kiểm tra hợp lệ: tổ hợp dịch vụ/loại chủ sở hữu được phép; `ownerId` là UUID.
- Trạng thái: `400 VALIDATION_ERROR`, `401 UNAUTHENTICATED`, `403 OWNER_ACCESS_DENIED`.
- Sử dụng `display_order` để có thứ tự sắp xếp xác định được.
