# Media Upload and Usage

## Mục đích

Admin upload media và dùng media đó làm thumbnail, attachment, hoặc nội dung nhúng trong Course, Lesson, hay Notification.

## Actor

Admin; Media Service; Course Service hoặc Notification Service.

## Điều kiện đầu vào

- Admin đã được xác thực.
- File thỏa MIME allowlist và size limit của Media Service.
- Owner (`Course`, `Lesson`, hoặc `Notification`) tồn tại và Admin có quyền cập nhật owner đó.

## Luồng chính

1. Client gửi multipart file đến `POST /api/media`.
2. Media Service validate MIME type, kích thước, và ghi binary vào MinIO.
3. Media Service tạo `media_objects`, trả `mediaId` và URL `/api/media/{mediaId}/content`.
4. Client chèn URL vào Markdown hoặc chọn media làm thumbnail/attachment.
5. Course hoặc Notification Service lưu Markdown source của owner.
6. Owner service gọi `POST /api/media/usages` với `mediaId`, owner, và `usageType`.
7. Media Service validate owner contract và lưu `media_usages`.
8. Khi render, client lấy media qua Media Service; `object_key` không bao giờ xuất hiện trong response public.

## Quy tắc thumbnail

- Chỉ một usage active `COURSE_THUMBNAIL` cho một Course.
- Đổi thumbnail soft-delete usage cũ rồi tạo usage mới.
- Course Service không lưu `thumbnail_media_id` và không gọi MinIO.

## Trường hợp lỗi

- `400`: file quá lớn, MIME type không hợp lệ, hoặc metadata thiếu.
- `403`: actor không có quyền tạo usage cho owner.
- `404`: media hoặc owner không tồn tại.
- `409`: media đã bị xóa hoặc cố tạo thumbnail trùng.

## Dữ liệu thay đổi

- Media Service database: `media_objects`, `media_usages`.
- MinIO: một object binary.
- Course/Notification Service: chỉ thay đổi Markdown của owner khi nội dung có thay đổi.
