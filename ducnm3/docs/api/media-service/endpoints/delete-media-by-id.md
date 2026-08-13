# Xóa phương tiện

## Tiêu chuẩn phản hồi

Phản hồi JSON thành công sử dụng cấu trúc bao dùng chung trong [`../shared/response-format.md`](../../shared/response-format.md). JSON cụ thể bên dưới là giá trị của `data`; thêm `meta` cho `traceId` và thông tin phân trang. Các điểm cuối truyền CSV và dữ liệu nhị phân theo luồng là ngoại lệ.

`DELETE /api/media/{mediaId}`

Dữ liệu phản hồi thành công `204 No Content`.

- Kiểm tra hợp lệ: `mediaId` là UUID; chỉ người tải lên hoặc quản trị viên được phép xóa.
- Trạng thái: `400 VALIDATION_ERROR`, `401 UNAUTHENTICATED`, `403 FORBIDDEN`, `404 MEDIA_NOT_FOUND`, `409 MEDIA_IN_USE`.
- Xóa mềm siêu dữ liệu và chỉ lên lịch/thực thi việc xóa đối tượng sau khi không còn thông tin sử dụng nào đang hoạt động.
