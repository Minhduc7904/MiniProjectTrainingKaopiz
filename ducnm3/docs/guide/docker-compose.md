# Hướng dẫn Docker Compose và Swagger dùng chung

## Điều kiện tiên quyết

- Docker Engine và plugin Docker Compose đang chạy.
- Thực hiện trong thư mục `ducnm3/`, nơi chứa `docker-compose.yml` và `.env`.

## Khởi động toàn bộ hệ thống

```bash
docker compose up --build
```

Chạy ở chế độ nền:

```bash
docker compose up -d --build
```

Dừng các container và mạng:

```bash
docker compose down
```

## Cổng dịch vụ

- API Gateway và Swagger UI dùng chung: `http://localhost:5100`
- Course Service: `http://localhost:5101`
- Student Service: `http://localhost:5102`
- Media Service: `http://localhost:5103`
- Notification Service: `http://localhost:5104`
- Scheduler Service: `http://localhost:5105`
- MinIO API: `http://localhost:9000`
- Bảng điều khiển MinIO: `http://localhost:9001`
- RabbitMQ AMQP: `localhost:5672`
- RabbitMQ management UI: `http://localhost:15672`

Gateway định tuyến các yêu cầu bên ngoài theo tiền tố dịch vụ:

- `/course/{path}` → Course Service
- `/student/{path}` → Student Service
- `/media/{path}` → Media Service
- `/notification/{path}` → Notification Service
- `/scheduler/{path}` → Scheduler Service

Ví dụ, trạng thái sức khỏe của Course Service có tại `http://localhost:5100/course/health`.

Các command Media đã triển khai dùng:

- upload công khai `POST /media/api/media` → service path `POST /api/media`;
- tạo avatar usage công khai `POST /media/api/media/usages` → service path
  `POST /api/media/usages`.

Gateway giữ nguyên multipart body khi bỏ tiền tố `/media`.

## Swagger UI dùng chung

`api-gateway` cung cấp một NSwag UI duy nhất tại `http://localhost:5100/swagger`. Dùng trình chọn tài liệu để tải API của từng dịch vụ:

- Course Service
- Student Service
- Media Service
- Notification Service
- Scheduler Service

Gateway chuyển tiếp từng tài liệu qua cùng một origin:

- `/course/swagger/v1/swagger.json`
- `/student/swagger/v1/swagger.json`
- `/media/swagger/v1/swagger.json`
- `/notification/swagger/v1/swagger.json`
- `/scheduler/swagger/v1/swagger.json`

Không cần cấu hình CORS từ trình duyệt đến dịch vụ vì giao diện và tài liệu đều được cung cấp qua Gateway.

## Cấu hình môi trường

`.env` chứa cấu hình và thông tin xác thực cho môi trường phát triển cục bộ:

```dotenv
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8080
Swagger__Enabled=true
SCHEDULER_DB_NAME=lms_scheduler_db
SCHEDULER_DB_USER=scheduler_app
SCHEDULER_DB_PASSWORD=replace-with-a-local-secret
MINIO_ROOT_USER=minio-root-user
MINIO_ROOT_PASSWORD=replace-with-a-long-root-secret
MINIO_APP_ACCESS_KEY=media-storage-app
MINIO_APP_SECRET_KEY=replace-with-a-long-app-secret
MINIO_IMAGE_BUCKET=images
MINIO_VIDEO_BUCKET=videos
MINIO_DOCUMENT_BUCKET=documents
MINIO_AUDIO_BUCKET=audios
MINIO_OTHER_BUCKET=other
MEDIA_UPLOAD_IMAGE_MAX_BYTES=10485760
MEDIA_UPLOAD_VIDEO_MAX_BYTES=524288000
MEDIA_UPLOAD_DOCUMENT_MAX_BYTES=52428800
MEDIA_UPLOAD_AUDIO_MAX_BYTES=104857600
MEDIA_UPLOAD_OTHER_MAX_BYTES=26214400
MEDIA_UPLOAD_REQUEST_MAX_BYTES=550502400
RABBITMQ_USER=lms_app
RABBITMQ_PASSWORD=replace-with-a-long-rabbitmq-secret
RABBITMQ_VHOST=/
MESSAGING_RETRY_COUNT=3
```

Git bỏ qua `.env`. Tuyệt đối không đặt thông tin xác thực môi trường sản xuất trong `.env.example`;
hãy dùng kho lưu trữ bí mật của hệ thống triển khai bên ngoài môi trường phát triển cục bộ.

## Cấp phát MinIO

`minio` lưu dữ liệu đối tượng trong volume bền vững `minio-data`. `minio-init`
đợi điểm cuối sức khỏe của MinIO, tạo năm bucket của Media Service và cấp phát
người dùng ứng dụng theo nguyên tắc đặc quyền tối thiểu. Có thể chạy lại tập lệnh
khởi tạo một cách an toàn và Media Service sẽ đợi tập lệnh hoàn tất.

Upload ghi database `PENDING` trước khi gọi MinIO, tính checksum SHA-256 trong
stream rồi chuyển `READY`; lỗi được compensation bằng xóa object và chuyển
`FAILED` theo best effort. Scheduler cleanup cho hàng `PENDING` stale chưa được
triển khai.

Chỉ chạy các thành phần phụ thuộc về lưu trữ:

```bash
docker compose up -d minio minio-init
```

## RabbitMQ và centralized retry

`rabbitmq` lưu broker data trong volume `rabbitmq-data`. Các API và
`scheduler-worker` chỉ start sau khi broker healthcheck pass. Cấu hình retry,
prefetch và concurrency nằm một lần trong `.env` qua nhóm biến
`MESSAGING_RETRY_*`, `MESSAGING_PREFETCH_COUNT` và
`MESSAGING_CONCURRENCY_LIMIT`; consumer không có retry riêng.

Riêng `notification-worker` có ba cấu hình performance, đều mặc định an toàn là
`1`/`1`/`120`:

- `NOTIFICATION_BATCH_DISPATCH_CHUNK_CONCURRENCY`: số chunk batch chạy cùng
  lúc; Compose đồng thời ánh xạ giá trị này vào
  `Messaging__Consumer__ConcurrencyLimit` của Notification Worker.
- `NOTIFICATION_BATCH_MAX_CONCURRENT_SENDS`: số recipient sender xử lý cùng
  lúc trong một chunk.
- `NOTIFICATION_BATCH_CLAIM_LEASE_SECONDS`: thời gian lease cho item
  `PROCESSING`; không đặt thấp hơn thời gian xử lý worst-case của một chunk.

Tăng lần lượt 1 → 2 → 4 sau benchmark; không tăng chỉ
`MESSAGING_CONCURRENCY_LIMIT` vì số dispatch command seed phải cùng giá trị.

Mở management UI bằng credential `RABBITMQ_USER`/`RABBITMQ_PASSWORD` để xem
exchange, queue và các queue `_error`.

## HTTP query giữa Media và Student

`media-service` nhận cả messaging environment và `HTTP_QUERY_*` environment
anchor. Typed client dùng `ServiceEndpoints__student-service`; timeout/retry
được quản lý tập trung bởi `BuildingBlocks.Http`, không cấu hình riêng trong
`StudentLookupClient`.

Xóa các container nhưng giữ nguyên dữ liệu:

```bash
docker compose down
```

Xóa các container cùng các volume phát triển MySQL/MinIO/RabbitMQ:

```bash
docker compose down -v
```

## Cấu hình tạo dữ liệu theo lựa chọn

`data-seeder` dùng cấu hình Compose `seed`, vì vậy lệnh `docker compose up` thông
thường không bao giờ tạo dữ liệu phát triển. Chỉ chạy công cụ này qua tập lệnh bao
bọc có cơ chế bảo vệ:

```bash
scripts/seed/run-development-seed.sh --confirm
```

Tập lệnh bao bọc chuẩn bị schema Student/Course đã áp dụng migration và chạy
container dòng lệnh một lần. Xem [`DATA_SEED_GUIDE.md`](DATA_SEED_GUIDE.md).

## Khắc phục sự cố

Kiểm tra trạng thái container:

```bash
docker compose ps
```

Xem toàn bộ nhật ký:

```bash
docker compose logs --follow
```

Dựng lại một dịch vụ:

```bash
docker compose build media-service
docker compose up -d media-service
```
