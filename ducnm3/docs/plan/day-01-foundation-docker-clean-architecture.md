# NGÀY 1 — Nền tảng, Docker, Kiến trúc sạch và cơ sở vận hành

## Kết quả thực tế

Ngày 1 gồm 7 task, được tách theo lịch sử thay đổi trên nhánh `ducnm3` ngày
12/08/2026: bốn commit được push trực tiếp và ba pull request đã merge. Các
liên kết dưới đây là nguồn truy vết cho từng task.

## Task đã hoàn thành

### 1. Khởi tạo thư mục project và README

- [x] Tạo thư mục `ducnm3/` và README khởi đầu cho project.
- Nguồn: [commit `8a7951a7` — create folder ducnm3](https://bitbucket.kaopiz.com/projects/SBUIN/repos/intern_be/commits/8a7951a724f3775ee1a7e1e332743996e266cce3).

### 2. Thiết lập khung tài liệu và kế hoạch triển khai

- [x] Tạo cấu trúc `docs/`, tài liệu kiến trúc, business flow, database,
  development guide, runbook và kế hoạch Ngày 1–5.
- [x] Thêm các rule và skill khởi đầu cho quy trình phát triển.
- Nguồn: [commit `7f3ead5e` — setup prj and docs](https://bitbucket.kaopiz.com/projects/SBUIN/repos/intern_be/commits/7f3ead5efb6b18740e8d037962b5a0c34d8161d8).

### 3. Dựng solution backend, Clean Architecture và Docker Compose

- [x] Tạo `backend/Lms.sln`, Course, Student, Media, Notification Service,
  API Gateway và worker khởi đầu; mỗi service có các lớp Domain,
  Application, Infrastructure và API.
- [x] Thêm Building Blocks dùng chung, Dockerfile, Docker Compose, MySQL
  bootstrap, SQL migration runner và script scaffold.
- [x] Thiết lập cấu hình môi trường, API documentation và migration guide ban
  đầu.
- Nguồn: [commit `0d8e80ff` — setup backend and db](https://bitbucket.kaopiz.com/projects/SBUIN/repos/intern_be/commits/0d8e80ffa3deeb8025acea6319663138cbc321c3).

### 4. Khởi tạo schema nghiệp vụ và persistence model

- [x] Thêm SQL schema cho Course, Student, Media và Notification database.
- [x] Scaffold `DbContext` và persistence model cho các bảng nghiệp vụ ban đầu.
- Nguồn: [commit `9d06764b` — init table](https://bitbucket.kaopiz.com/projects/SBUIN/repos/intern_be/commits/9d06764bd40daab4cf528aa67901b83b3bec5394).

### 5. Chuẩn hóa API contract, health check, Gateway và kiểm thử nền tảng

- [x] Bổ sung response/error envelope dùng chung, correlation ID, global
  exception middleware và health API có database probe.
- [x] Cấu hình Gateway Swagger/YARP cho các service và bổ sung NUnit test
  project cùng các test cho Building Blocks và health endpoint.
- Nguồn: [PR #140 — add service health API contracts](https://bitbucket.kaopiz.com/projects/SBUIN/repos/intern_be/pull-requests/140/overview).

### 6. Xây nền tảng lưu trữ MinIO cho Media Service

- [x] Thêm `IStorage`, `IStorageHealthProbe` và `MinioStorageService`; chỉ
  Media Service sở hữu MinIO SDK và credential.
- [x] Khởi tạo năm bucket media, validation MIME/category/kích thước, object
  key UTC, Media health check và kiểm thử unit/component/integration với MinIO.
- Nguồn: [PR #141 — Add Media MinIO storage foundation](https://bitbucket.kaopiz.com/projects/SBUIN/repos/intern_be/pull-requests/141/overview).

### 7. Thêm Scheduler Service và làm sạch migration baseline

- [x] Tạo Scheduler API, Worker, database, health probe và route Gateway.
- [x] Chuẩn hóa mỗi database về migration `V001`, scaffold lại model
  Notification/Scheduler và thêm workflow reset database development an toàn.
- Nguồn: [PR #142 — add Scheduler service and clean migration baseline](https://bitbucket.kaopiz.com/projects/SBUIN/repos/intern_be/pull-requests/142/overview).

## Tiêu chí hoàn thành Ngày 1

- [x] Solution backend, Clean Architecture, database-first migration và Docker
  Compose đã được thiết lập.
- [x] API Gateway, API contract dùng chung và health check cho các service đã
  có sẵn.
- [x] Media Service có nền tảng MinIO; Scheduler Service có khung API/Worker
  và database riêng.
- [x] Có nền tảng NUnit test, tài liệu kiến trúc/vận hành và script hỗ trợ phát
  triển.

## Chưa thuộc phạm vi hoàn thành Ngày 1

- [ ] API CRUD nghiệp vụ cho khóa học, bài học, ghi danh, tiến độ hoặc học viên.
- [ ] HTTP API tải lên/tải xuống và ghi metadata media trong use case nghiệp vụ.
- [ ] Quy trình gửi thông báo, retry/idempotency nghiệp vụ và vòng lặp thực thi
  Scheduler.
- [ ] Seed dữ liệu, benchmark N+1, kiểm thử hiệu năng, tích hợp liên service,
  kiểm thử đầu cuối và frontend.
