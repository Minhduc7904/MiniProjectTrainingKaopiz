# NGÀY 1 — Nền tảng, Docker, Kiến trúc sạch và cơ sở vận hành

## Kết quả cuối ngày

Ngày 1 đã hoàn thành toàn bộ nền tảng hiện có của LMS. Hệ thống đã có solution
ASP.NET Core, bốn service nghiệp vụ và một service nền tảng Scheduler theo Kiến
trúc sạch, API Gateway, cách tiếp cận database-first với SQL migration,
Docker Compose cho MySQL và MinIO, hợp đồng API dùng chung, kiểm tra trạng thái,
bộ chuyển đổi lưu trữ và kiểm thử nền tảng.

Các trường hợp sử dụng nghiệp vụ như CRUD khóa học/bài học, ghi danh, gửi thông
báo, tải lên/tải xuống media qua HTTP, tối ưu N+1, seed dữ liệu và giao diện vẫn thuộc các
ngày tiếp theo; chúng chưa được xem là hoàn thành trong Ngày 1.

## 1. Solution và kiến trúc đã hoàn thành

- [x] Tạo solution `backend/Lms.sln`.
- [x] Tạo Course, Student, Media, Notification và Scheduler Service.
- [x] Mỗi service có bốn lớp:

  ```text
  <Service>Service.Domain
  <Service>Service.Application
  <Service>Service.Infrastructure
  <Service>Service.Api
  ```

- [x] Giữ dependency đúng chiều: Domain không phụ thuộc framework; Application
  không phụ thuộc ASP.NET Core/MySQL; Infrastructure triển khai port của
  Application; API là composition root.
- [x] Tạo API Gateway `Lms.ApiGateway` bằng YARP.
- [x] Tạo khung `SchedulerService.Worker` thay cho worker chuyên biệt của Notification; giai đoạn này chưa poll, claim, phân tích CRON hoặc gọi service khác.
- [x] Tạo các building block dùng chung:

  ```text
  BuildingBlocks.Shared
  BuildingBlocks.Contracts
  BuildingBlocks.DatabaseMigration
  BuildingBlocks.Presentation
  ```

- [x] Chuẩn hóa thiết lập biên dịch .NET trong `backend/Directory.Build.props`.
- [x] Tổ chức các script hỗ trợ project tại thư mục gốc `scripts/`:

  ```text
  scripts/database/bootstrap/
  scripts/database/tools/
  scripts/storage/minio/
  ```

## 2. Database-first và MySQL đã hoàn thành

- [x] Mỗi service sở hữu database riêng:

  ```text
  Course Service        lms_course_db
  Student Service       lms_student_db
  Media Service         lms_media_db
  Notification Service  lms_notification_db
  Scheduler Service     lms_scheduler_db
  ```

- [x] Toàn bộ credential và connection string lấy từ biến môi trường;
  `.env` được bỏ qua và `.env.example` chỉ chứa giá trị giữ chỗ.
- [x] SQL migration là nguồn chuẩn; không sử dụng EF Core migration để
  quản lý schema.
- [x] Có trình chạy migration dùng `schema_migrations`, sắp xếp theo phiên bản,
  checksum, MySQL lock, log migration và dừng ngay khi migration lỗi.
- [x] Có `mysql-init` khởi tạo năm database cùng người dùng ứng dụng tương ứng
  sau khi MySQL đạt trạng thái `healthy`.
- [x] Mỗi service có thư mục migration riêng và không tạo foreign key xuyên
  service database.
- [x] Đã có SQL schema và các model persistence được scaffold cho:

  | Dịch vụ | Bảng nghiệp vụ |
  | --- | --- |
  | Course | `courses`, `lessons`, `enrollments`, `lesson_progresses` |
  | Student | `students` |
  | Media | `media_objects`, `media_usages` |
  | Notification | `notification_batches`, `notification_batch_items`, `notifications` |
  | Scheduler | `background_jobs`, `background_job_runs` |

- [x] Đặt lại sạch lịch sử migration: mỗi database có một migration `V001`, có
  script đặt lại với chốt bảo vệ giữ nguyên MinIO, và Notification/Scheduler đã được
  scaffold lại.
- [x] Có các script chạy migration/scaffold cục bộ và
  [`MIGRATION_GUIDE.md`](../guide/MIGRATION_GUIDE.md) mô tả workflow Database
  First an toàn.

## 3. Docker Compose và môi trường cục bộ đã hoàn thành

- [x] Có Dockerfile dùng chung để biên dịch từng ASP.NET Core service.
- [x] `docker-compose.yml` khởi động MySQL, `mysql-init`, MinIO, `minio-init`,
  năm API service và API Gateway.
- [x] MySQL dùng volume bền vững `mysql-data`, healthcheck và múi giờ UTC.
- [x] MinIO dùng volume bền vững `minio-data`, cổng API `9000`, cổng console
  `9001` và healthcheck `/minio/health/live`.
- [x] Các API service chờ `mysql-init`; Media Service chờ cả `mysql-init` và
  `minio-init`.
- [x] Các cổng phát triển công khai:

  | Thành phần | Cổng |
  | --- | --- |
  | API Gateway | `5100` |
  | Course Service | `5101` |
  | Student Service | `5102` |
  | Media Service | `5103` |
  | Notification Service | `5104` |
  | Scheduler Service | `5105` |
  | MySQL | `3306` |
  | MinIO API / Console | `9000` / `9001` |

## 4. OpenAPI, Gateway và hành vi API dùng chung đã hoàn thành

- [x] Mỗi API service có tài liệu NSwag OpenAPI và Swagger UI riêng.
- [x] Gateway chuyển tiếp yêu cầu đến Course, Student, Media, Notification và Scheduler bằng YARP.
- [x] Gateway cung cấp Swagger UI tại `/swagger` với bộ chọn tài liệu cho từng
  service, không gộp endpoint của nhiều service vào một tài liệu.
- [x] Gateway proxy tài liệu OpenAPI theo các route `/course`, `/student`,
  `/media`, `/notification`, `/scheduler`.
- [x] Chuẩn hóa cấu trúc bao thành công/lỗi của API, `traceId`, mã tương quan, mã
  lỗi dùng chung, tên header, path và hằng số trạng thái health.
- [x] Có middleware ngoại lệ toàn cục; lỗi từ service phía sau Gateway trả
  `503 SERVICE_UNAVAILABLE` theo cấu trúc bao lỗi dùng chung.

## 5. Kiểm tra trạng thái service đã hoàn thành

- [x] Tất cả service có `GET /health` dùng định dạng phản hồi chung.
- [x] Course, Student, Notification và Scheduler thực hiện `SELECT 1` trên database sở hữu
  và trả `503 DATABASE_UNAVAILABLE` khi database lỗi.
- [x] Kiểm tra trạng thái Media chạy thăm dò cơ sở dữ liệu và lưu trữ song song:

  | Database | MinIO | Kết quả |
  | --- | --- | --- |
  | healthy | healthy | `200` với `database.status` và `storage.status` |
  | unhealthy | healthy | `503 DATABASE_UNAVAILABLE` |
  | healthy | unhealthy | `503 STORAGE_UNAVAILABLE` |
  | unhealthy | unhealthy | `503 DEPENDENCY_UNAVAILABLE` |

## 6. Nền tảng MinIO trong Media Service đã hoàn thành

- [x] Chỉ Media Service có MinIO SDK và thông tin xác thực; Course/Student/Notification
  không truy cập MinIO.
- [x] Application sở hữu `IStorage` và `IStorageHealthProbe`; Infrastructure
  triển khai `MinioStorageService`.
- [x] Bộ chuyển đổi lưu trữ hỗ trợ tải lên/tải xuống dạng luồng, kiểm tra tồn tại,
  metadata và xóa.
- [x] Có năm cặp category/bucket cố định:

  | Loại | Bucket |
  | --- | --- |
  | `IMAGE` | `images` |
  | `VIDEO` | `videos` |
  | `DOCUMENT` | `documents` |
  | `AUDIO` | `audios` |
  | `OTHER` | `other` |

- [x] Object key được sinh theo UTC:

  ```text
  yyyy/MM/dd/{uuid}.{extension}
  ```

- [x] Kiểm tra hợp lệ MIME/category, phần mở rộng, stream có thể đọc, kích thước
  và bucket được phép trước khi gọi MinIO; ngoại lệ SDK được bọc bằng ngoại lệ
  lưu trữ của application.
- [x] `minio-init` tạo năm bucket theo cách idempotent, tạo người dùng ứng dụng
  chuyên biệt cho Media và policy chỉ có quyền trên năm bucket này.
- [x] Chưa mở endpoint HTTP tải lên/tải xuống hay ghi `media_objects` trong trường
  hợp sử dụng; đây là giới hạn chủ đích của nền tảng Ngày 1.

## 7. Nền tảng kiểm thử đã hoàn thành

- [x] Dùng NUnit cho kiểm thử backend.
- [x] Kiểm thử Shared Presentation bao phủ mã tương quan, cấu trúc bao ngoại lệ,
  endpoint kiểm tra trạng thái database dùng chung và ánh xạ lỗi Gateway Swagger.
- [x] Mỗi service, gồm Scheduler, có project kiểm thử đơn vị cho thao tác hủy database health probe.
- [x] Kiểm thử đơn vị/component Media bao phủ option MinIO, ánh xạ bucket, key UTC,
  kiểm tra hợp lệ MIME/category/kích thước và bốn tổ hợp trạng thái phụ thuộc.
- [x] Kiểm thử tích hợp Media dùng MinIO Testcontainer cô lập để kiểm tra:

  ```text
  Tải lên -> Kiểm tra tồn tại -> Metadata -> Tải xuống -> Xóa
  ```

  cho cả năm category media, đồng thời kiểm tra trạng thái storage.

- [x] Danh mục kiểm thử được tổ chức theo `docs/tests/<service>/<type>.md`; mỗi
  tài liệu ghi rõ trường hợp kiểm thử, bước chuẩn bị, kết quả mong đợi và điều
  kiện đạt.

## 8. Tài liệu và vận hành đã hoàn thành

- [x] Có tài liệu kiến trúc, mô hình dữ liệu database, rich media, định dạng
  phản hồi/xử lý lỗi API, Docker Compose, MinIO, migration và kiểm thử.
- [x] Tài liệu API tách theo service/endpoint.
- [x] Có `AGENTS.md`, rules, skills structure và Git workflow guide để agents
  và developer xác định đúng phạm vi làm việc.

## Tiêu chí hoàn thành Ngày 1

- [x] `docker compose config` hợp lệ.
- [x] `docker compose up -d --build` đã khởi động toàn bộ hệ thống.
- [x] MySQL đạt trạng thái `healthy`, khởi tạo năm database/user và SQL migration chạy thành
  công.
- [x] MinIO đạt trạng thái `healthy`, console truy cập tại `http://localhost:9001`, đã khởi tạo
  đủ năm Media bucket.
- [x] Các API Course, Student, Media, Notification, Scheduler và Gateway được biên dịch/chạy bằng
  Docker.
- [x] Swagger riêng của service và bộ chọn tài liệu Gateway Swagger hoạt động.
- [x] Media `/health` xác nhận đồng thời MySQL và MinIO; sự cố lưu trữ trả
  đúng `503 STORAGE_UNAVAILABLE`.
- [x] `dotnet build backend/Lms.sln -m:1` thành công.
- [x] Kiểm thử đơn vị/component/tích hợp NUnit backend đã đạt.

## Chưa thuộc phạm vi hoàn thành Ngày 1

- [ ] API CRUD nghiệp vụ cho khóa học, bài học, ghi danh, tiến độ hoặc học viên.
- [ ] HTTP API tải lên/tải xuống hoặc URL ký trước cho media.
- [ ] Trường hợp sử dụng ghi metadata `media_objects`/`media_usages`.
- [ ] Quy trình gửi thông báo, xử lý theo lô/thử lại/tính idempotent nghiệp vụ.
- [ ] Vòng lặp thực thi Scheduler, bộ phân tích CRON, claim lần chạy và lời gọi service nội bộ.
- [ ] Benchmark N+1, seed dữ liệu, kiểm thử hiệu năng, tích hợp liên service,
  kiểm thử đầu cuối và frontend.
