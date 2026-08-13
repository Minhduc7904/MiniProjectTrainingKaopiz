# Tải lên và sử dụng media

## Mục đích

Quản trị viên tải media lên và dùng media đó làm ảnh đại diện, tệp đính kèm hoặc nội dung nhúng trong Khóa học, Bài học hay Thông báo.

## Tác nhân

Quản trị viên; Media Service; Course Service hoặc Notification Service.

## Điều kiện đầu vào

- Quản trị viên đã được xác thực.
- Tệp thỏa danh sách MIME được phép và giới hạn kích thước của Media Service.
- Đối tượng sở hữu (Khóa học, Bài học hoặc Thông báo) tồn tại và Quản trị viên có quyền cập nhật đối tượng đó.

## Luồng chính

1. Ứng dụng khách gửi tệp multipart đến `POST /api/media`.
2. Media Service xác thực loại MIME, kích thước và ghi dữ liệu nhị phân vào MinIO.
3. Media Service tạo `media_objects`, trả `mediaId` và URL `/api/media/{mediaId}/content`.
4. Ứng dụng khách chèn URL vào Markdown hoặc chọn media làm ảnh đại diện/tệp đính kèm.
5. Course Service hoặc Notification Service lưu mã nguồn Markdown của đối tượng sở hữu.
6. Dịch vụ sở hữu gọi `POST /api/media/usages` với `mediaId`, đối tượng sở hữu và `usageType`.
7. Media Service xác thực hợp đồng đối tượng sở hữu và lưu `media_usages`.
8. Khi hiển thị, ứng dụng khách lấy media qua Media Service; `object_key` không bao giờ xuất hiện trong phản hồi công khai.

## Quy tắc ảnh đại diện

- Chỉ một lượt sử dụng `COURSE_THUMBNAIL` được hoạt động cho mỗi Khóa học.
- Khi đổi ảnh đại diện, lượt sử dụng cũ được xóa mềm rồi lượt sử dụng mới được tạo.
- Course Service không lưu `thumbnail_media_id` và không gọi MinIO.

## Trường hợp lỗi

- `400`: tệp quá lớn, loại MIME không hợp lệ hoặc thiếu siêu dữ liệu.
- `403`: tác nhân không có quyền tạo lượt sử dụng cho đối tượng sở hữu.
- `404`: media hoặc đối tượng sở hữu không tồn tại.
- `409`: media đã bị xóa hoặc cố tạo ảnh đại diện trùng.

## Dữ liệu thay đổi

- Cơ sở dữ liệu Media Service: `media_objects`, `media_usages`.
- MinIO: một đối tượng nhị phân.
- Course Service/Notification Service: chỉ thay đổi Markdown của đối tượng sở hữu khi nội dung thay đổi.
