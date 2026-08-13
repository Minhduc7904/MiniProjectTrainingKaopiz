# Notification Markdown and Embedded Media

## Mục đích

Admin soạn nội dung notification bằng Markdown và nhúng image, video, document, audio, hoặc media khác an toàn.

## Actor

Admin; Media Service; Notification Service; Student.

## Điều kiện đầu vào

- Admin có quyền gửi notification.
- Media đã được upload thành công vào Media Service.
- Renderer có Markdown sanitizer và chỉ cho phép URL do Media Service cấp.

## Luồng chính

1. Admin upload media đến Media Service và nhận `mediaId` cùng URL content.
2. Admin soạn `bodyMarkdown`, ví dụ `![Thông báo](/api/media/{mediaId}/content)`.
3. Notification Service validate Markdown, từ chối raw HTML, script, và URL scheme không an toàn.
4. Notification Service gọi Media Service để tạo `media_usages` với:
   - `owner_service = NOTIFICATION`
   - `owner_type = NOTIFICATION_BODY`
   - `usage_type = EMBED` hoặc `ATTACHMENT`
5. Notification được gửi đơn hoặc dùng làm nội dung của batch job.
6. Student mở inbox; client render Markdown đã sanitize và tải media từ Media Service.

## Trường hợp lỗi

- `400`: Markdown không hợp lệ hoặc URL không thuộc Media Service.
- `403`: Admin không có quyền dùng media.
- `404`: media đã bị xóa hoặc không còn active.
- `409`: media usage đã tồn tại.

## Dữ liệu thay đổi

- Media Service database: `media_usages`.
- Notification Service database: `notifications` hoặc `notification_batches`.
- MinIO không bị gọi bởi Notification Service.
