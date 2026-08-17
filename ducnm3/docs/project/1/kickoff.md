# Phase 1 — Kickoff

## In scope

| Nhóm | Phạm vi |
| --- | --- |
| Functional | Course/Lesson/Enrollment/Progress, Student query, Media upload/content/usage, Notification/batch và Scheduler data boundary. |
| Technical | ASP.NET Core, Clean Architecture, YARP, MySQL, MinIO (Media ownership), RabbitMQ/MassTransit, Docker Compose, Worker, response/error/correlation và NUnit/Testcontainers. |
| Performance | Kế hoạch benchmark batch, CSV streaming, N+1, index/query plan, pagination và API; chưa là claim runtime. |

### Scope chi tiết từ yêu cầu Lead

- Course gồm export danh sách CSV 100k+ record; baseline materialize toàn bộ dữ
  liệu phải được so sánh với batch/streaming.
- Notification batch snapshot Học viên `ACTIVE` theo điều kiện, xử lý nền theo
  page/chunk và không gửi toàn bộ recipient trong một HTTP request/transaction.
- Item gửi lỗi được retry đúng một lần; lần thứ hai thất bại phải lưu trạng
  thái/error an toàn và không tạo notification trùng khi redelivery.
- Performance experiment gồm batch 3k/10k/100k, index 10k/100k/1M, API
  10k/100k, N+1 Course → Lessons → Progress, query plan và pagination khi offset
  tăng.
- API xử lý nhất quán validation error, resource not found và
  database/dependency error; response có trace/correlation ID và không lộ lỗi
  nội bộ.

## Out of scope

Payment, chat, livestream, exam, certificate, recommendation AI, production-grade
Kubernetes và multi-region deployment. Bất cứ feature nào xuất hiện ở source sau
này phải được đánh giá lại trước khi giữ trong Out of Scope.

## Constraints & Dependencies

- Solo developer, timeline mini-project, local resource và demo-oriented.
- MySQL 8.4, MinIO, RabbitMQ, Docker Engine là runtime/development dependency.
- Notification phụ thuộc Student query và Media usage qua boundary; không có FK/query xuyên database.
- Benchmark cần cùng máy, dataset, cấu hình, warm-up (nếu dùng) và nhiều lần chạy.
- Lead chưa cung cấp SLO tuyệt đối; acceptance về performance dựa trên số đo
  Before/After cho đến khi có target được duyệt.

Chi tiết: [assumptions](registers/assumptions.md) và [risks](registers/risks.md).
