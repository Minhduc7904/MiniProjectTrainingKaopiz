# DAY 1 — Foundation, Docker, Clean Architecture và nền tảng vận hành

## Kết quả cuối ngày

Day 1 đã hoàn thành toàn bộ foundation hiện có của LMS. Hệ thống đã có solution
ASP.NET Core, bốn business service và một Scheduler platform service theo Clean Architecture, API Gateway, database
first với SQL migrations, Docker Compose cho MySQL và MinIO, shared API
contracts, health checks, storage adapter, và test nền tảng.

Các use case nghiệp vụ như CRUD Course/Lesson, enrollment, notification
delivery, HTTP upload/download media, N+1 optimization, seed data, và UI vẫn
thuộc các ngày tiếp theo; chúng chưa được xem là hoàn thành trong Day 1.

## 1. Solution và kiến trúc đã hoàn thành

- [x] Tạo solution `backend/Lms.sln`.
- [x] Tạo Course, Student, Media, Notification và Scheduler Service.
- [x] Mỗi service có bốn layer:

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
- [x] Tạo `SchedulerService.Worker` skeleton thay cho Notification-specific worker; phase này chưa poll, claim, parse CRON hoặc gọi service khác.
- [x] Tạo shared building blocks:

  ```text
  BuildingBlocks.Shared
  BuildingBlocks.Contracts
  BuildingBlocks.DatabaseMigration
  BuildingBlocks.Presentation
  ```

- [x] Chuẩn hoá .NET build settings trong `backend/Directory.Build.props`.
- [x] Tổ chức project helper scripts tại root `scripts/`:

  ```text
  scripts/database/bootstrap/
  scripts/database/tools/
  scripts/storage/minio/
  ```

## 2. Database First và MySQL đã hoàn thành

- [x] Mỗi service sở hữu database riêng:

  ```text
  Course Service        lms_course_db
  Student Service       lms_student_db
  Media Service         lms_media_db
  Notification Service  lms_notification_db
  Scheduler Service     lms_scheduler_db
  ```

- [x] Toàn bộ credential và connection string lấy từ environment variables;
  `.env` được ignored và `.env.example` chỉ chứa placeholder.
- [x] SQL migration là source of truth; không sử dụng EF Core migration để
  quản lý schema.
- [x] Có migration runner dùng `schema_migrations`, version ordering, checksum,
  MySQL lock, log migration, và fail-fast khi migration lỗi.
- [x] Có `mysql-init` bootstrap năm database cùng application user tương ứng
  sau khi MySQL healthy.
- [x] Mỗi service có migration folder riêng và không tạo foreign key xuyên
  service database.
- [x] Đã có SQL schema và scaffolded persistence models cho:

  | Service | Business tables |
  | --- | --- |
  | Course | `courses`, `lessons`, `enrollments`, `lesson_progresses` |
  | Student | `students` |
  | Media | `media_objects`, `media_usages` |
  | Notification | `notification_batches`, `notification_batch_items`, `notifications` |
  | Scheduler | `background_jobs`, `background_job_runs` |

- [x] Reset migration history sạch: mỗi database có một migration `V001`, có
  guarded reset script giữ nguyên MinIO, và Notification/Scheduler đã được
  scaffold lại.
- [x] Có scripts chạy migration/scaffold local và
  [`MIGRATION_GUIDE.md`](../guide/MIGRATION_GUIDE.md) mô tả workflow Database
  First an toàn.

## 3. Docker Compose và local environment đã hoàn thành

- [x] Có Dockerfile dùng chung để build từng ASP.NET Core service.
- [x] `docker-compose.yml` khởi động MySQL, `mysql-init`, MinIO, `minio-init`,
  năm API service và API Gateway.
- [x] MySQL dùng persistent volume `mysql-data`, healthcheck, và timezone UTC.
- [x] MinIO dùng persistent volume `minio-data`, API port `9000`, console port
  `9001`, và healthcheck `/minio/health/live`.
- [x] Các API service chờ `mysql-init`; Media Service chờ cả `mysql-init` và
  `minio-init`.
- [x] Các public development ports:

  | Component | Port |
  | --- | --- |
  | API Gateway | `5100` |
  | Course Service | `5101` |
  | Student Service | `5102` |
  | Media Service | `5103` |
  | Notification Service | `5104` |
  | Scheduler Service | `5105` |
  | MySQL | `3306` |
  | MinIO API / Console | `9000` / `9001` |

## 4. OpenAPI, Gateway và shared API behavior đã hoàn thành

- [x] Mỗi API service có NSwag OpenAPI document và Swagger UI riêng.
- [x] Gateway proxy request đến Course, Student, Media, Notification và Scheduler bằng YARP.
- [x] Gateway cung cấp Swagger UI tại `/swagger` với document selector cho từng
  service, không gộp endpoint của nhiều service vào một document.
- [x] Gateway proxy OpenAPI documents theo các route `/course`, `/student`,
  `/media`, `/notification`, `/scheduler`.
- [x] Chuẩn hoá API success/error envelope, `traceId`, correlation ID, shared
  error codes, header names, paths và health status constants.
- [x] Có global exception middleware; lỗi downstream Gateway trả
  `503 SERVICE_UNAVAILABLE` theo shared error envelope.

## 5. Service health checks đã hoàn thành

- [x] Tất cả service có `GET /health` dùng response format chung.
- [x] Course, Student, Notification và Scheduler thực hiện `SELECT 1` trên database sở hữu
  và trả `503 DATABASE_UNAVAILABLE` khi database lỗi.
- [x] Media health chạy database probe và storage probe song song:

  | Database | MinIO | Kết quả |
  | --- | --- | --- |
  | healthy | healthy | `200` với `database.status` và `storage.status` |
  | unhealthy | healthy | `503 DATABASE_UNAVAILABLE` |
  | healthy | unhealthy | `503 STORAGE_UNAVAILABLE` |
  | unhealthy | unhealthy | `503 DEPENDENCY_UNAVAILABLE` |

## 6. MinIO foundation trong Media Service đã hoàn thành

- [x] Chỉ Media Service có MinIO SDK và credential; Course/Student/Notification
  không truy cập MinIO.
- [x] Application sở hữu `IStorage` và `IStorageHealthProbe`; Infrastructure
  triển khai `MinioStorageService`.
- [x] Storage adapter hỗ trợ stream upload/download, exists, metadata và delete.
- [x] Có năm category/bucket cố định:

  | Category | Bucket |
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

- [x] Validate MIME/category, extension, readable stream, size và allowed
  bucket trước khi gọi MinIO; SDK exception được bọc bằng storage exception của
  application.
- [x] `minio-init` tạo idempotent năm bucket, dedicated Media application user,
  và policy chỉ có quyền trên năm bucket này.
- [x] Chưa mở HTTP endpoint upload/download hay ghi `media_objects` trong use
  case; đây là giới hạn chủ đích của foundation Day 1.

## 7. Test foundation đã hoàn thành

- [x] Dùng NUnit cho backend tests.
- [x] Shared Presentation tests bao phủ correlation ID, exception envelope,
  shared database health endpoint, và Gateway Swagger failure mapping.
- [x] Mỗi service, gồm Scheduler, có unit test project cho database health probe cancellation.
- [x] Media unit/component tests bao phủ MinIO options, bucket mapping, UTC key,
  MIME/category/size validation và bốn tổ hợp health dependencies.
- [x] Media integration test dùng MinIO Testcontainer cô lập để kiểm tra:

  ```text
  Upload -> Exists -> Metadata -> Download -> Delete
  ```

  cho cả năm media category, đồng thời kiểm tra storage health.

- [x] Test catalogue được tổ chức theo `docs/tests/<service>/<type>.md`; mỗi
  tài liệu ghi rõ test case, setup, expected result và điều kiện pass.

## 8. Tài liệu và vận hành đã hoàn thành

- [x] Có tài liệu architecture, database data model, rich media, API response
  format/error handling, Docker Compose, MinIO, migration và testing.
- [x] API documentation tách theo service/endpoint.
- [x] Có `AGENTS.md`, rules, skills structure và Git workflow guide để agents
  và developers xác định đúng scope làm việc.

## Definition of Done Day 1

- [x] `docker compose config` hợp lệ.
- [x] `docker compose up -d --build` đã khởi động full stack.
- [x] MySQL healthy, bootstrap năm database/user và SQL migrations chạy thành
  công.
- [x] MinIO healthy, console truy cập tại `http://localhost:9001`, provisioning
  đủ năm Media bucket.
- [x] Course, Student, Media, Notification, Scheduler APIs và Gateway được build/run bằng
  Docker.
- [x] Swagger service riêng và Gateway Swagger document selector hoạt động.
- [x] Media `/health` xác nhận đồng thời MySQL và MinIO; storage outage trả
  đúng `503 STORAGE_UNAVAILABLE`.
- [x] `dotnet build backend/Lms.sln -m:1` thành công.
- [x] Backend NUnit unit/component/integration tests đã pass.

## Chưa thuộc phạm vi hoàn thành Day 1

- [ ] Business CRUD APIs cho Course, Lesson, enrollment, progress hoặc student.
- [ ] HTTP API upload/download hoặc presigned URL cho media.
- [ ] Use case ghi metadata `media_objects`/`media_usages`.
- [ ] Notification delivery workflow, batch/retry/idempotency nghiệp vụ.
- [ ] Scheduler execution loop, CRON parser, run claiming và internal service calls.
- [ ] N+1 benchmark, seed data, performance test, cross-service integration,
  end-to-end test và frontend.
