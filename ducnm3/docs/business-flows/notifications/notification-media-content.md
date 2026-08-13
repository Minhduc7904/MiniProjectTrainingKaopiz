# Markdown của Thông báo và media nhúng

## Mục đích

Quản trị viên soạn nội dung thông báo bằng Markdown và nhúng hình ảnh, video, tài liệu, âm thanh hoặc loại media khác một cách an toàn.

## Tác nhân

Quản trị viên; Media Service; Notification Service; Học viên.

## Điều kiện đầu vào

- Quản trị viên có quyền gửi thông báo.
- Media đã được tải lên Media Service thành công.
- Bộ hiển thị có bộ làm sạch Markdown và chỉ cho phép URL do Media Service cấp.

## Luồng chính

1. Quản trị viên tải media lên Media Service và nhận `mediaId` cùng URL nội dung.
2. Quản trị viên soạn `bodyMarkdown`, ví dụ `![Thông báo](/api/media/{mediaId}/content)`.
3. Notification Service xác thực Markdown, từ chối HTML thô, mã lệnh và lược đồ URL không an toàn.
4. Notification Service gọi Media Service để tạo `media_usages` với:
   - `owner_service = NOTIFICATION`
   - `owner_type = NOTIFICATION_BODY`
   - `usage_type = EMBED` hoặc `ATTACHMENT`
5. Thông báo được gửi đơn lẻ hoặc dùng làm nội dung của tác vụ hàng loạt.
6. Học viên mở hộp thư đến; ứng dụng khách hiển thị Markdown đã được làm sạch và tải media từ Media Service.

## Trường hợp lỗi

- `400`: Markdown không hợp lệ hoặc URL không thuộc Media Service.
- `403`: Quản trị viên không có quyền dùng media.
- `404`: media đã bị xóa hoặc không còn hoạt động.
- `409`: lượt sử dụng media đã tồn tại.

## Dữ liệu thay đổi

- Cơ sở dữ liệu Media Service: `media_usages`.
- Cơ sở dữ liệu Notification Service: `notifications` hoặc `notification_batches`.
- MinIO không bị gọi bởi Notification Service.
