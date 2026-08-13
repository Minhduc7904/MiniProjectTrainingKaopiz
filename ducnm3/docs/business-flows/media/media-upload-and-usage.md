# Tải lên media và đặt avatar Học viên

## Mục đích

Học viên tải media lên qua Gateway và đăng ký một media đã sẵn sàng làm avatar.
Phiên bản hiện tại chỉ triển khai usage
`STUDENT/STUDENT_AVATAR/AVATAR`; các usage cho Khóa học, Bài học và Thông báo vẫn
là hướng mở rộng sau.

## Tác nhân

Học viên; API Gateway; Media Service; Student Service; MinIO.

## Điều kiện đầu vào

- `uploadedByType/uploadedBy` và `createdByType/createdBy` hiện được client gửi
  như request identity tạm thời vì hệ thống chưa có xác thực.
- Actor type hiện chỉ hỗ trợ `STUDENT`; actor ID phải tồn tại trong Student
  Service. Khi có JWT, hai cặp field này sẽ được lấy từ claim thay vì request.
- Tệp có tên/phần mở rộng, MIME khớp `mediaType` và kích thước nằm trong giới hạn
  Media Service.
- Media chỉ được dùng làm avatar sau khi có trạng thái `READY`.

Actor và owner là hai khái niệm riêng: `createdByType/createdBy` cho biết ai tạo
usage, còn `ownerService/ownerType/ownerId` cho biết tài nguyên sở hữu media.
`ownerType=STUDENT_AVATAR` không phải actor type.

## Luồng upload

1. Client gửi multipart đến Gateway
   `POST /media/api/media`; Gateway bỏ tiền tố `/media` và chuyển đến service
   path `POST /api/media`.
2. Media Service kiểm tra `file`, `mediaType`, `uploadedByType`, `uploadedBy` và
   gọi `GET /api/students/{studentId}` tại Student Service để xác minh actor.
3. Media Service cấp phát bucket/object key nội bộ và ghi một hàng
   `media_objects` trạng thái `PENDING` **trước** khi gọi MinIO.
4. Media Service stream file vào MinIO. Adapter tính checksum SHA-256 trong lúc
   đọc stream, không cần tải toàn bộ file vào bộ nhớ.
5. Khi object và checksum hợp lệ, Media Service cập nhật hàng thành `READY`, lưu
   `checksum_sha256`, `completed_at`, rồi trả `201` cùng `mediaId`.
6. Bucket, object key, credential và `failure_reason` không xuất hiện trong
   response công khai.

## Luồng tạo usage avatar

1. Client gửi JSON đến Gateway `POST /media/api/media/usages`; service path là
   `POST /api/media/usages`.
2. Media Service xác minh actor từ `createdByType/createdBy`. Nếu actor không
   phải chính `ownerId`, service cũng tra cứu Học viên sở hữu tại Student
   Service.
3. Media Service yêu cầu `mediaId` tồn tại, chưa bị xóa và có trạng thái
   `READY`.
4. Trong transaction `SERIALIZABLE`, usage avatar active cũ của `ownerId` được
   xóa mềm và usage mới được tạo.
5. Generated column cùng unique index bảo đảm tối đa một
   `STUDENT/STUDENT_AVATAR/AVATAR` active cho mỗi Học viên khi request chạy đồng
   thời.

## Compensation và cleanup

- Nếu MinIO upload lỗi/bị hủy, không trả checksum hoặc bước chuyển `READY` lỗi,
  Media Service cố gắng xóa object và chuyển hàng database sang `FAILED`.
- Hai thao tác compensation đều là best effort để không che lỗi gốc.
- Process có thể dừng sau khi ghi `PENDING` nhưng trước khi hoàn tất
  compensation. Quét và xử lý các hàng `PENDING` stale được hoãn cho Scheduler;
  job cleanup này chưa được triển khai.

## Trường hợp lỗi

- `400`: multipart/JSON field hoặc UUID không hợp lệ, actor type/usage tuple
  không được hỗ trợ.
- `404`: actor, owner hoặc media không tồn tại.
- `409`: media chưa `READY` hoặc xung đột unique usage active.
- `413`: file vượt giới hạn.
- `415`: MIME không khớp `mediaType`.
- `503`: Student Service/MinIO không khả dụng hoặc upload thất bại.

## Dữ liệu thay đổi

- Database Media Service: `media_objects` đi qua
  `PENDING -> READY | FAILED`; `media_usages` xóa mềm avatar cũ và tạo avatar
  mới.
- MinIO: tạo object khi upload; xóa object khi compensation có thể thực hiện.
- Database Student Service: chỉ đọc để xác minh actor/owner, không thay đổi.
