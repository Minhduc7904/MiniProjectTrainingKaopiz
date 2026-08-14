# Hướng dẫn thiết lập môi trường phát triển

Hướng dẫn này dành cho người mới chạy backend LMS trên máy cục bộ. Cách nhanh
nhất là chạy toàn bộ hệ thống bằng Docker Compose; không cần cài MySQL hoặc MinIO
trực tiếp trên máy.

## 1. Cài công cụ cần thiết

Bắt buộc:

- Git.
- Docker Engine (Docker Desktop trên Windows/macOS, Docker Engine trên Linux).
- Tiện ích Docker Compose.

Khuyến nghị nếu cần biên dịch/kiểm thử/scaffold trực tiếp trên máy:

- .NET SDK `10.0`.
- Node.js LTS (kèm `npm`) để chạy `frontend/lms-web`.

Kiểm tra sau khi cài:

```bash
git --version
docker --version
docker compose version
dotnet --version
```

`dotnet` không bắt buộc nếu chỉ chạy Docker Compose. Tuy nhiên, cần có công cụ
này khi chạy kiểm thử đơn vị/tích hợp, script migration hoặc EF Core scaffold
trên máy cục bộ.

## 2. Sao chép project

```bash
git clone https://bitbucket.kaopiz.com/scm/sbuin/intern_be.git
cd intern_be/ducnm3
```

Mọi lệnh trong hướng dẫn này được chạy từ thư mục `ducnm3/`, nơi có
`docker-compose.yml` và `.env.example`.

## 3. Kiểm tra cổng trước khi khởi động

Docker Compose cần các cổng sau chưa được sử dụng:

| Thành phần | Cổng |
| --- | --- |
| API Gateway | `5100` |
| Course / Student / Media / Notification / Scheduler API | `5101` / `5102` / `5103` / `5104` / `5105` |
| MySQL | `3306` |
| MinIO API / Console | `9000` / `9001` |

Ví dụ kiểm tra cổng trên Linux:

```bash
ss -ltn
```

Tìm các cổng trong bảng ở đầu ra. Nếu có cổng đang được sử dụng, dừng
process/container đang chiếm cổng hoặc đổi ánh xạ cổng trong
`docker-compose.yml` và các connection string cục bộ liên quan.

## 4. Tạo cấu hình cục bộ

Tạo `.env` từ template:

```bash
cp .env.example .env
```

Mở `.env` và thay toàn bộ giá trị `replace-with-...` bằng thông tin xác thực cục bộ của
bạn. Các giá trị cần nhất quán:

- `*_DB_USER`, `*_DB_PASSWORD` và connection string tương ứng phải cùng user,
  password và database.
- `MYSQL_ROOT_PASSWORD` chỉ dùng để khởi tạo MySQL cục bộ.
- `MINIO_ROOT_USER` / `MINIO_ROOT_PASSWORD` là tài khoản quản trị MinIO cục bộ.
- `MINIO_APP_ACCESS_KEY` / `MINIO_APP_SECRET_KEY` là tài khoản giới hạn quyền
  mà Media Service sử dụng.
- `MEDIA_UPLOAD_*_MAX_BYTES` đặt giới hạn từng loại media;
  `MEDIA_UPLOAD_REQUEST_MAX_BYTES` phải đủ chứa giới hạn video và multipart
  overhead.
- `RABBITMQ_USER` / `RABBITMQ_PASSWORD` là credential của application và
  management UI cục bộ. Nhóm `MESSAGING_RETRY_*` là retry policy dùng chung cho
  mọi consumer.

Không commit `.env`. Tệp này đã được bỏ qua; chỉ `.env.example` được commit.

## 5. Kiểm tra cấu hình Docker Compose

Trước khi khởi động, kiểm tra Docker Compose đã đọc đủ các biến môi trường:

```bash
docker compose config --quiet
```

Lệnh không in lỗi nghĩa là cấu hình Compose hợp lệ. Lỗi
`must be set` thường có nghĩa `.env` thiếu một biến bắt buộc hoặc biến đó để
trống.

## 6. Khởi động toàn bộ hệ thống

Biên dịch image và chạy nền:

```bash
docker compose up -d --build
```

Trong lần chạy đầu, Docker sẽ tải image .NET, MySQL, MinIO và RabbitMQ nên mất nhiều
thời gian hơn các lần sau.

Compose khởi động theo thứ tự dependency:

1. `mysql` đạt trạng thái `healthy`.
2. `mysql-init` tạo năm database và các người dùng ứng dụng.
3. `minio` đạt trạng thái `healthy`.
4. `minio-init` tạo năm bucket cùng user/policy ứng dụng cho Media Service.
5. `rabbitmq` đạt trạng thái `healthy`.
6. Các API service chạy SQL migration, kết nối broker và chỉ báo ready khi cả
   database/RabbitMQ đều healthy.
7. `scheduler-worker` khởi động MassTransit host.
8. `api-gateway` proxy các API service.

**Không scaffold EF Core khi API khởi động.** Scaffold chỉ là thao tác phát triển
thủ công sau khi thay đổi SQL schema.

Kiểm tra các container:

```bash
docker compose ps
```

`mysql`, `minio` và `rabbitmq` phải có trạng thái `healthy`. `mysql-init` và `minio-init`
là container chạy một lần nên trạng thái thành công là `Exited (0)`.

Theo dõi log toàn hệ thống:

```bash
docker compose logs --follow
```

Theo dõi một service, ví dụ Media:

```bash
docker compose logs --follow media-service
```

## 7. Kiểm tra sau khi chạy

Mở các URL sau:

| Mục đích | URL |
| --- | --- |
| Gateway Swagger | <http://localhost:5100/swagger> |
| Gateway health | <http://localhost:5100/health> |
| Trạng thái Course qua Gateway | <http://localhost:5100/course/health> |
| Trạng thái Student qua Gateway | <http://localhost:5100/student/health> |
| Trạng thái Media qua Gateway | <http://localhost:5100/media/health> |
| Trạng thái Notification qua Gateway | <http://localhost:5100/notification/health> |
| Trạng thái Scheduler qua Gateway | <http://localhost:5100/scheduler/health> |
| MinIO console | <http://localhost:9001> |
| RabbitMQ management UI | <http://localhost:15672> |

Đăng nhập console MinIO bằng `MINIO_ROOT_USER` và `MINIO_ROOT_PASSWORD` trong
`.env`. Sau khi khởi tạo thành công, có năm bucket:

```text
images
videos
documents
audios
other
```

Ví dụ kiểm tra trạng thái Media bằng lệnh:

```bash
curl --fail-with-body http://localhost:5100/media/health
```

Kết quả thành công có `database.status` và `storage.status` đều là `healthy`.

Để thử upload qua Gateway, dùng một UUID Học viên đã tồn tại:

```bash
curl -X POST http://localhost:5100/media/api/media \
  -F "file=@avatar.png;type=image/png" \
  -F "mediaType=IMAGE" \
  -F "uploadedByType=STUDENT" \
  -F "uploadedBy=<student-uuid>"
```

Sau khi lấy `data.id`, đăng ký avatar:

```bash
curl -X POST http://localhost:5100/media/api/media/usages \
  -H "Content-Type: application/json" \
  -d '{
    "mediaId": "<media-uuid>",
    "ownerService": "STUDENT",
    "ownerType": "STUDENT_AVATAR",
    "ownerId": "<student-uuid>",
    "usageType": "AVATAR",
    "displayOrder": 0,
    "createdByType": "STUDENT",
    "createdBy": "<student-uuid>"
  }'
```

Các actor field hiện là request identity tạm thời chưa được authentication bảo
vệ và sẽ chuyển sang lấy từ JWT. Không dùng chúng như cơ chế phân quyền production.

## 8. Dừng, chạy lại và đặt lại dữ liệu

Dừng các container nhưng giữ toàn bộ dữ liệu MySQL/MinIO:

```bash
docker compose down
```

Chạy lại hệ thống đã biên dịch:

```bash
docker compose up -d
```

Biên dịch lại sau khi sửa mã nguồn:

```bash
docker compose up -d --build
```

Đặt lại đúng năm database MySQL phát triển nhưng giữ nguyên MinIO:

```bash
scripts/database/reset-development-databases.sh --confirm
docker compose up -d --build
```

Tập lệnh chỉ chạy với `ASPNETCORE_ENVIRONMENT=Development`, tên database cục bộ
chuẩn và xác nhận rõ ràng. Chỉ xóa cả MySQL lẫn lưu trữ đối tượng khi muốn đặt lại
toàn bộ môi trường:

```bash
docker compose down -v
docker compose up -d --build
```

`down -v` xóa `mysql-data` và `minio-data`, không thể khôi phục dữ liệu cục bộ
đã xóa.

## 9. Seed dữ liệu phát triển theo yêu cầu

Mặc định seed 100k học viên, 100k khóa học, 1-5 bài học/khóa học và 1-10
khóa học/học viên:

```bash
scripts/seed/run-development-seed.sh --confirm
```

Seeder là profile Compose chạy một lần, không tự chạy cùng hệ thống ứng dụng và
không nằm trong migration. Giao diện terminal hiển thị tiến độ, tốc độ và ETA.
Nếu lần chạy bị gián đoạn, dùng cùng option/random seed kèm `--resume`; để xóa
toàn bộ dữ liệu seed, chạy thao tác đặt lại database có chốt bảo vệ ở mục 8.

Xem chế độ chạy thử, tập dữ liệu nhỏ, kiểm tra an toàn và cách khắc phục sự cố tại
[`../guide/DATA_SEED_GUIDE.md`](../guide/DATA_SEED_GUIDE.md).

## 10. Biên dịch và kiểm thử trên máy cục bộ

Cần .NET SDK `10.0`:

```bash
dotnet build backend/Lms.sln -m:1
dotnet test backend/Lms.sln -m:1
```

Kiểm thử tích hợp Media tự tạo một MinIO Testcontainer cô lập; Docker phải đang
chạy:

```bash
dotnet test backend/Services/Media/MediaService.IntegrationTests/MediaService.IntegrationTests.csproj
```

Xem chi tiết từng trường hợp kiểm thử tại [`../tests/README.md`](../tests/README.md).

## 11. Khi thêm hoặc đổi SQL migration

Migration SQL là nguồn chuẩn. Sau khi tạo migration mới trong service sở hữu,
có hai cách áp dụng:

- Chạy `docker compose up -d --build`: service tự áp dụng migration đang chờ
  khi khởi động.
- Hoặc chạy thủ công từ máy cục bộ:

  ```bash
  set -a
  . ./.env
  set +a
  sh scripts/database/tools/migrate.sh media
  ```

Sau khi migration thành công, scaffold lại model của đúng service:

```bash
set -a
. ./.env
set +a
sh scripts/database/tools/scaffold.sh media
dotnet build backend/Lms.sln -m:1
```

Thay `media` bằng `course`, `student`, `notification`, hoặc `scheduler` khi cần. Không sửa
một migration đã được áp dụng; tạo migration mới để sửa schema. Xem thêm
[`../guide/MIGRATION_GUIDE.md`](../guide/MIGRATION_GUIDE.md).

## 12. Chạy frontend

```bash
cd frontend/lms-web
cp .env.example .env
npm install
npm run dev
```

App lắng nghe cổng `5173` và gọi Gateway tại `VITE_API_BASE_URL`
(`http://localhost:5100` mặc định). `VITE_API_TIMEOUT_MS` mặc định `5000` để
khớp timeout HttpClient của Gateway. `VITE_HTTP_LOG=true` in từng request/response
Axios ra DevTools. Folder, Redux và rule page nằm ở
`docs/architecture/frontend.md`. Gateway CORS cho origin Vite nằm ở
`Cors:AllowedOrigins` trong `Lms.ApiGateway/appsettings.json`.

## 13. Các giới hạn hiện tại

Nền tảng đã chạy được và Media Service đã có HTTP command upload/tạo avatar
usage, nhưng vẫn còn các giới hạn:

- Actor của hai command Media do request cung cấp tạm thời; chưa có JWT.
- Usage hiện chỉ hỗ trợ avatar Học viên; usage Khóa học/Thông báo và các endpoint
  đọc/xóa media khác chưa thuộc phần triển khai này.
- Compensation đồng bộ đã có, nhưng cleanup hàng `PENDING` stale được hoãn cho
  Scheduler và chưa có job thực thi.
- Chưa có CRUD khóa học/bài học/ghi danh/tiến độ.
- Chưa có worker gửi thông báo hoặc quy trình xử lý theo lô/thử lại/tính idempotent.
- Scheduler Worker mới là khung cơ bản; chưa phân tích CRON, claim lần chạy, thực thi handler
  hoặc gọi Media/Notification Service.
- Frontend đã có khung `lms-web` (Vite, Tailwind, Axios, Redux) và trang sổ học viên;
  chưa có kiểm thử đầu cuối.

## 14. Khắc phục sự cố nhanh

| Triệu chứng | Cách kiểm tra / xử lý |
| --- | --- |
| `port is already allocated` | Dừng process/container đang chiếm cổng hoặc đổi ánh xạ cổng. |
| `must be set` khi Compose chạy | Kiểm tra `.env`, đặc biệt là credential MySQL, `MINIO_*` và `RABBITMQ_*`. |
| API không khởi động sau migration | Chạy `docker compose logs <service>`; sửa migration mới, không sửa migration đã áp dụng. |
| `mysql-init` hoặc `minio-init` thất bại | Xem log container chạy một lần: `docker compose logs mysql-init` hoặc `docker compose logs minio-init`. |
| Media `/health` trả `STORAGE_UNAVAILABLE` | Kiểm tra `docker compose ps`, `docker compose logs minio minio-init`, cấu hình bucket và credential `MINIO_APP_*`. |
| API `/health` trả `DEPENDENCY_UNAVAILABLE` | Kiểm tra `docker compose ps rabbitmq`, `docker compose logs rabbitmq` và credential `RABBITMQ_*`. |
| Muốn làm sạch toàn bộ môi trường cục bộ | Chỉ thực hiện khi chấp nhận mất dữ liệu: `docker compose down -v`. |
