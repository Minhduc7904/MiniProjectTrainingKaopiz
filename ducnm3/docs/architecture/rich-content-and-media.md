# Thiết kế nội dung đa dạng và media

## Ranh giới sở hữu

Chỉ Media Service sở hữu:

- Thông tin xác thực MinIO và thao tác tải lên, tải xuống, xóa đối tượng.
- Siêu dữ liệu `media_objects` và các liên kết `media_usages`.
- Việc xác thực tệp, giới hạn kích thước, danh sách MIME được phép và URL phân phối nội dung.

Course Service sở hữu Markdown của Khóa học/Bài học. Notification Service sở hữu Markdown của Thông báo. Cả hai dịch vụ đều không được dùng SDK MinIO hoặc truy vấn cơ sở dữ liệu của Media Service.

Student Service sở hữu hồ sơ Học viên. Media Service gọi endpoint query
`GET /api/students/{studentId}` qua typed HTTP client để xác minh actor và owner
của avatar; Media Service không truy vấn database Student.

## Ranh giới lưu trữ

`MediaService.Application` định nghĩa các cổng `IStorage` và
`IStorageHealthProbe` mà không phụ thuộc vào MinIO. `MediaService.Infrastructure`
sở hữu bộ chuyển đổi SDK MinIO, việc xác thực cấu hình, ánh xạ danh mục sang vùng chứa,
tạo khóa đối tượng và chuyển đổi ngoại lệ SDK. `MediaService.Api` chỉ kết hợp các
phụ thuộc này và cung cấp trạng thái vận hành.

Các vùng chứa được cấu hình gồm `images`, `videos`, `documents`, `audios` và
`other`. Khóa đối tượng tải lên tuân theo mẫu `yyyy/MM/dd/{uuid}.{extension}` theo
UTC. Khi khởi động, Media Service xác thực điểm cuối, thông tin xác thực, thời
gian chờ và năm tên vùng chứa riêng biệt. Việc khởi tạo vùng chứa vẫn là thao tác hạ
tầng do `minio-init` thực hiện, không phải do tiến trình ứng dụng.

## Các trường Markdown

| Đối tượng sở hữu | Trường | Mục đích |
|---|---|---|
| Khóa học | `description_markdown` | Mô tả đa dạng của một Khóa học |
| Bài học | `content_markdown` | Nội dung Bài học đa dạng |
| Lô Thông báo | `body_markdown` | Mẫu dùng để tạo thông báo hàng loạt |
| Thông báo | `body_markdown` | Nội dung hộp thư đến được hiển thị cho một Học viên |

Giá trị được lưu là mã nguồn Markdown. Bộ hiển thị phải loại bỏ HTML thô, JavaScript, các trình xử lý sự kiện nội tuyến và những lược đồ URL không an toàn.

## Vòng đời upload hiện tại

1. Client gửi multipart đến Gateway `POST /media/api/media`; Gateway chuyển tới
   Media Service path `POST /api/media`.
2. Media Service xác minh request actor qua Student Service, cấp phát location
   nội bộ rồi ghi `media_objects` trạng thái `PENDING` trước khi gọi MinIO.
3. Adapter MinIO stream object và tính checksum SHA-256. Sau khi thành công,
   Media Service lưu checksum và chuyển hàng sang `READY`.
4. Chỉ media `READY`, chưa bị xóa mềm mới được tham chiếu bởi usage.
5. Khi upload/lấy checksum/hoàn tất database lỗi, Media Service cố gắng xóa
   object và chuyển hàng sang `FAILED`. Đây là compensation best effort.
6. Việc thu hồi hàng `PENDING` stale khi process dừng giữa luồng được hoãn cho
   Scheduler; job này chưa được triển khai.

Request upload hiện không idempotent. Mỗi lần gửi thành công tạo một `mediaId`
và object riêng. Bucket, object key và `failure_reason` chỉ là dữ liệu nội bộ.

`GET /api/media/{mediaId}/content` dùng Application capability chỉ public
metadata và thao tác `CopyToAsync`; bucket/object key vẫn nằm trong adapter
boundary. API stream thẳng MinIO vào response, không buffer toàn file. Phiên bản
hiện tại dùng `Cache-Control: no-store`, chưa có auth, range request hoặc
presigned URL.

## Cấp URL theo usage

`IMediaUrlProvider` là abstraction Application để cấp URL ảnh cho một hoặc nhiều
`media_usages` active. Implementation hiện tại sinh Gateway content URL; khi cần
presigned URL, thay implementation bằng adapter Infrastructure dùng metadata nội
bộ của media, không đổi handler, endpoint hoặc contract của service gọi. Các API
URL luôn nhận usage/owner, không nhận bucket hoặc object key.

## Request identity tạm thời

Hệ thống chưa có authentication middleware cho hai command Media. Vì vậy:

- upload nhận `uploadedByType/uploadedBy`;
- tạo usage nhận `createdByType/createdBy`;
- hai cặp field được validate bằng actor strategy hiện chỉ hỗ trợ `STUDENT`;
- khi có JWT, actor sẽ được ánh xạ từ claim thay vì tin cậy field do client gửi.

Actor fields mô tả người thực hiện thao tác và được lưu riêng. Chúng khác
`ownerService/ownerType/ownerId`, là logical reference tới tài nguyên sở hữu.
Đặc biệt, `createdByType=STUDENT` không đồng nghĩa với
`ownerType=STUDENT_AVATAR`.

## Ngữ nghĩa sử dụng

Mọi đường tạo usage đi qua core `EnsureMediaUsagesAsync`: dedupe theo unique
active reference, xác thực media `READY`, tạo idempotent trong transaction hiện
có của outbox hoặc một transaction `SERIALIZABLE` mới. Policy theo type giữ phần
khác nhau: avatar/thumbnail thay usage active cũ; thumbnail derivation cập nhật
trạng thái derivation; notification body chỉ thêm usage. Nhờ đó phần persistence
chung không bị sao chép giữa các luồng.

- `AVATAR`: phiên bản hiện tại hỗ trợ
  `STUDENT/STUDENT_AVATAR/AVATAR`. Khi thay avatar, usage active cũ được xóa mềm
  và usage mới được tạo trong transaction `SERIALIZABLE`. Generated
  `active_reference_guard` chỉ áp dụng unique reference cho usage active, nên
  chuỗi thay avatar A → B → A hợp lệ nhưng duplicate active vẫn bị chặn.
- `THUMBNAIL`: mỗi Khóa học chỉ có đúng một lượt sử dụng `COURSE_THUMBNAIL` đang hoạt động; đây là nguồn dữ liệu chuẩn cho hình ảnh hiển thị.
- `EMBED`: media được hiển thị nội tuyến trong Markdown.
- `ATTACHMENT`: media có thể tải xuống, được liên kết với đối tượng sở hữu nhưng không hiển thị nội tuyến.

`owner_service`, `owner_type` và `owner_id` trong `media_usages` là các tham chiếu logic giữa các dịch vụ. Chúng được chủ đích không thiết lập làm khóa ngoại của cơ sở dữ liệu.

Generated column `active_student_avatar_owner_id` cùng unique index bảo đảm mỗi
Học viên có tối đa một avatar active, kể cả khi có request đồng thời. Các usage
Khóa học/Thông báo vẫn có trong mô hình dữ liệu định hướng; command tạo usage
hiện tại chưa chấp nhận các tổ hợp đó.

## Phân phối thông báo

- Một thông báo đơn lẻ ghi một hàng `notifications` cho một người nhận.
- Một lô gửi hàng loạt chụp lại danh sách người nhận và ghi `notification_batch_items`.
- Bộ xử lý phân phối tạo một hàng `notifications` cho mỗi mục thành công.
- Trong một dispatch chunk, các notification thành công cùng Markdown được gom thành `RegisterNotificationMediaUsageBatchV1`. Media Worker xác thực media dùng chung một lần và tạo một usage idempotent cho mỗi `notification.id`.
- Bộ xử lý phân phối dùng `(notification_batch_id, recipient_student_id)` làm khóa lũy đẳng, ngăn tạo mục hộp thư đến thứ hai sau khi thử lại hoặc khởi động lại.
