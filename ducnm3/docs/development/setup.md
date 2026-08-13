# Development Setup Guide

Hướng dẫn này dành cho người mới chạy backend LMS trên máy local. Cách nhanh
nhất là chạy toàn bộ stack bằng Docker Compose; không cần cài MySQL hoặc MinIO
trực tiếp trên máy.

## 1. Cài công cụ cần thiết

Bắt buộc:

- Git.
- Docker Engine (Docker Desktop trên Windows/macOS, Docker Engine trên Linux).
- Docker Compose plugin.

Khuyến nghị nếu cần build/test/scaffold trực tiếp trên máy:

- .NET SDK `10.0`.

Kiểm tra sau khi cài:

```bash
git --version
docker --version
docker compose version
dotnet --version
```

`dotnet` không bắt buộc để chỉ chạy Docker Compose. Tuy nhiên cần có nó khi
chạy unit/integration test, migration script hoặc EF Core scaffold ở local.

## 2. Clone project

```bash
git clone https://bitbucket.kaopiz.com/scm/sbuin/intern_be.git
cd intern_be/ducnm3
```

Mọi lệnh trong guide này được chạy từ thư mục `ducnm3/`, nơi có
`docker-compose.yml` và `.env.example`.

## 3. Kiểm tra port trước khi khởi động

Docker Compose cần các port sau chưa được dùng:

| Component | Port |
| --- | --- |
| API Gateway | `5100` |
| Course / Student / Media / Notification / Scheduler API | `5101` / `5102` / `5103` / `5104` / `5105` |
| MySQL | `3306` |
| MinIO API / Console | `9000` / `9001` |

Trên Linux, kiểm tra port ví dụ:

```bash
ss -ltn
```

Tìm các port trong bảng ở output. Nếu có port đang dùng, dừng process/container
đang chiếm port hoặc đổi mapping port trong `docker-compose.yml` và các local
connection string liên quan.

## 4. Tạo cấu hình local

Tạo `.env` từ template:

```bash
cp .env.example .env
```

Mở `.env` và thay toàn bộ giá trị `replace-with-...` bằng credential local của
bạn. Các giá trị cần nhất quán:

- `*_DB_USER`, `*_DB_PASSWORD` và connection string tương ứng phải cùng user,
  password và database.
- `MYSQL_ROOT_PASSWORD` chỉ dùng để bootstrap MySQL local.
- `MINIO_ROOT_USER` / `MINIO_ROOT_PASSWORD` là tài khoản quản trị MinIO local.
- `MINIO_APP_ACCESS_KEY` / `MINIO_APP_SECRET_KEY` là tài khoản giới hạn quyền
  mà Media Service sử dụng.

Không commit `.env`. File này đã được ignore; chỉ `.env.example` được commit.

## 5. Kiểm tra cấu hình Docker Compose

Trước khi khởi động, kiểm tra Docker Compose đã đọc đủ environment variables:

```bash
docker compose config --quiet
```

Lệnh không in lỗi nghĩa là cấu hình Compose hợp lệ. Lỗi
`must be set` thường có nghĩa `.env` thiếu một biến bắt buộc hoặc biến đó để
trống.

## 6. Khởi động toàn bộ hệ thống

Build image và chạy nền:

```bash
docker compose up -d --build
```

Trong lần chạy đầu, Docker sẽ pull image .NET, MySQL và MinIO nên mất nhiều
thời gian hơn các lần sau.

Compose khởi động theo thứ tự dependency:

1. `mysql` healthy.
2. `mysql-init` tạo năm database và application users.
3. `minio` healthy.
4. `minio-init` tạo năm buckets và Media Service application user/policy.
5. Các API services chạy SQL migration của database sở hữu.
6. `api-gateway` proxy các API services.

**Không scaffold EF Core khi API khởi động.** Scaffold chỉ là thao tác development
thủ công sau khi thay đổi SQL schema.

Kiểm tra các container:

```bash
docker compose ps
```

`mysql` và `minio` phải có trạng thái `healthy`. `mysql-init` và `minio-init`
là one-shot container nên trạng thái thành công là `Exited (0)`.

Theo dõi log toàn stack:

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
| Course health qua Gateway | <http://localhost:5100/course/health> |
| Student health qua Gateway | <http://localhost:5100/student/health> |
| Media health qua Gateway | <http://localhost:5100/media/health> |
| Notification health qua Gateway | <http://localhost:5100/notification/health> |
| Scheduler health qua Gateway | <http://localhost:5100/scheduler/health> |
| MinIO console | <http://localhost:9001> |

Đăng nhập MinIO console bằng `MINIO_ROOT_USER` và `MINIO_ROOT_PASSWORD` trong
`.env`. Sau khi provisioning thành công, có năm buckets:

```text
images
videos
documents
audios
other
```

Ví dụ kiểm tra Media health bằng command:

```bash
curl --fail-with-body http://localhost:5100/media/health
```

Kết quả thành công có `database.status` và `storage.status` đều là `healthy`.

## 8. Dừng, chạy lại và reset dữ liệu

Dừng containers nhưng giữ toàn bộ MySQL/MinIO data:

```bash
docker compose down
```

Chạy lại stack đã build:

```bash
docker compose up -d
```

Build lại sau khi sửa code:

```bash
docker compose up -d --build
```

Reset đúng năm MySQL development database nhưng giữ nguyên MinIO:

```bash
scripts/database/reset-development-databases.sh --confirm
docker compose up -d --build
```

Script chỉ chạy với `ASPNETCORE_ENVIRONMENT=Development`, tên database local
chuẩn và confirmation rõ ràng. Xóa cả MySQL lẫn object storage chỉ khi muốn
reset toàn bộ môi trường:

```bash
docker compose down -v
docker compose up -d --build
```

`down -v` xóa `mysql-data` và `minio-data`, không thể khôi phục dữ liệu local
đã xóa.

## 9. Seed development data theo yêu cầu

Seed mặc định 100k Students, 100k Courses, 1-5 Lessons/Course và 1-10
Courses/Student:

```bash
scripts/seed/run-development-seed.sh --confirm
```

Seeder là one-shot Compose profile, không tự chạy cùng application stack và
không nằm trong migration. UI terminal hiển thị progress, tốc độ và ETA. Nếu
run bị gián đoạn, dùng cùng options/random seed kèm `--resume`; để xóa toàn bộ
seed data, chạy guarded database reset ở mục 8.

Xem dry run, dataset nhỏ, safety checks và troubleshooting tại
[`../guide/DATA_SEED_GUIDE.md`](../guide/DATA_SEED_GUIDE.md).

## 10. Chạy build và test trên máy local

Cần .NET SDK `10.0`:

```bash
dotnet build backend/Lms.sln -m:1
dotnet test backend/Lms.sln -m:1
```

Media integration tests tự tạo một MinIO Testcontainer cô lập; Docker phải đang
chạy:

```bash
dotnet test backend/Services/Media/MediaService.IntegrationTests/MediaService.IntegrationTests.csproj
```

Xem chi tiết từng test case tại [`../tests/README.md`](../tests/README.md).

## 11. Khi thêm hoặc đổi SQL migration

Migration SQL là source of truth. Sau khi tạo migration mới trong service sở
hữu, có hai cách apply:

- Chạy `docker compose up -d --build`: service tự apply pending migration khi
  start.
- Hoặc chạy thủ công từ host:

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
một migration đã được apply; tạo migration mới để sửa schema. Xem thêm
[`../guide/MIGRATION_GUIDE.md`](../guide/MIGRATION_GUIDE.md).

## 12. Các giới hạn hiện tại

Foundation đã chạy được, nhưng các HTTP use case nghiệp vụ chưa hoàn thành:

- Chưa có CRUD Course/Lesson/enrollment/progress.
- Chưa có HTTP endpoint upload/download media hoặc presigned URL.
- Chưa có notification delivery worker, batch/retry/idempotency workflow.
- Scheduler Worker mới là skeleton; chưa parse CRON, claim run, execute handler
  hoặc gọi Media/Notification Service.
- Chưa có frontend hoặc end-to-end test.

Do đó Swagger hiện chủ yếu phục vụ health/info endpoints và các API sẽ được bổ
sung ở các giai đoạn tiếp theo.

## 13. Troubleshooting nhanh

| Triệu chứng | Cách kiểm tra / xử lý |
| --- | --- |
| `port is already allocated` | Dừng process/container đang chiếm port hoặc đổi port mapping. |
| `must be set` khi Compose chạy | Kiểm tra `.env`, đặc biệt MySQL và `MINIO_*` credentials. |
| API không start sau migration | Chạy `docker compose logs <service>`; sửa migration mới, không sửa migration đã apply. |
| `mysql-init` hoặc `minio-init` fail | Xem log one-shot container: `docker compose logs mysql-init` hoặc `docker compose logs minio-init`. |
| Media `/health` trả `STORAGE_UNAVAILABLE` | Kiểm tra `docker compose ps`, `docker compose logs minio minio-init`, bucket config và `MINIO_APP_*` credentials. |
| Muốn làm sạch toàn bộ environment local | Chỉ khi chấp nhận mất data: `docker compose down -v`. |
