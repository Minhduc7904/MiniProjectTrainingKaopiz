# Thiết kế nội dung đa dạng và media

## Ranh giới sở hữu

Chỉ Media Service sở hữu:

- Thông tin xác thực MinIO và thao tác tải lên, tải xuống, xóa đối tượng.
- Siêu dữ liệu `media_objects` và các liên kết `media_usages`.
- Việc xác thực tệp, giới hạn kích thước, danh sách MIME được phép và URL phân phối nội dung.

Course Service sở hữu Markdown của Khóa học/Bài học. Notification Service sở hữu Markdown của Thông báo. Cả hai dịch vụ đều không được dùng SDK MinIO hoặc truy vấn cơ sở dữ liệu của Media Service.

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

## Vòng đời media

Các ca sử dụng tải lên/tải xuống qua HTTP trong vòng đời này mới ở giai đoạn lập
kế hoạch; phần triển khai hiện tại chỉ cung cấp cổng lưu trữ và bộ chuyển đổi.

1. Ứng dụng khách tải dữ liệu multipart lên Media Service.
2. Media Service xác thực tệp, ghi tệp vào MinIO và tạo `media_objects`.
3. Ứng dụng khách nhận `mediaId` cùng URL nội dung rồi đưa URL vào Markdown, ví dụ `![Hình ảnh Khóa học](/api/media/{mediaId}/content)`.
4. Course Service hoặc Notification Service lưu mã nguồn Markdown.
5. Dịch vụ sở hữu gọi Media Service để tạo `media_usages` cho từng ảnh đại diện, tệp đính kèm hoặc nội dung nhúng.
6. Khi nội dung loại bỏ một tham chiếu, dịch vụ sở hữu xóa lượt sử dụng tương ứng. Media Service có thể thu gom media không còn lượt sử dụng hoạt động sau thời gian lưu giữ.

Khóa học không lưu `thumbnail_media_id`. Để hiển thị ảnh đại diện, Khóa học yêu cầu Media Service trả về lượt sử dụng đang hoạt động với `ownerService=COURSE`, `ownerType=COURSE_THUMBNAIL` và `ownerId={courseId}`.

## Ngữ nghĩa sử dụng

- `THUMBNAIL`: mỗi Khóa học chỉ có đúng một lượt sử dụng `COURSE_THUMBNAIL` đang hoạt động; đây là nguồn dữ liệu chuẩn cho hình ảnh hiển thị.
- `EMBED`: media được hiển thị nội tuyến trong Markdown.
- `ATTACHMENT`: media có thể tải xuống, được liên kết với đối tượng sở hữu nhưng không hiển thị nội tuyến.

`owner_service`, `owner_type` và `owner_id` trong `media_usages` là các tham chiếu logic giữa các dịch vụ. Chúng được chủ đích không thiết lập làm khóa ngoại của cơ sở dữ liệu.

## Phân phối thông báo

- Một thông báo đơn lẻ ghi một hàng `notifications` cho một người nhận.
- Một lô gửi hàng loạt chụp lại danh sách người nhận và ghi `notification_batch_items`.
- Bộ xử lý phân phối Thông báo trong tương lai tạo một hàng `notifications` cho mỗi mục thành công.
- Bộ xử lý phân phối trong tương lai dùng `(notification_batch_id, recipient_student_id)` làm khóa lũy đẳng, ngăn tạo mục hộp thư đến thứ hai sau khi thử lại hoặc khởi động lại.
