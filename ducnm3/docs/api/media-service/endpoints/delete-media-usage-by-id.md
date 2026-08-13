# Xóa thông tin sử dụng phương tiện

## Tiêu chuẩn phản hồi

Phản hồi JSON thành công sử dụng cấu trúc bao dùng chung trong [`../shared/response-format.md`](../../shared/response-format.md). JSON cụ thể bên dưới là giá trị của `data`; thêm `meta` cho `traceId` và thông tin phân trang. Các điểm cuối truyền CSV và dữ liệu nhị phân theo luồng là ngoại lệ.

`DELETE /api/media/usages/{usageId}`

Dữ liệu phản hồi thành công `204 No Content`.

- Kiểm tra hợp lệ: `usageId` là UUID; bên gọi sở hữu chủ thể của thông tin sử dụng.
- Trạng thái: `400 VALIDATION_ERROR`, `401 UNAUTHENTICATED`, `403 OWNER_ACCESS_DENIED`, `404 MEDIA_USAGE_NOT_FOUND`.
- Xóa mềm thông tin sử dụng; không xóa đối tượng phương tiện.
