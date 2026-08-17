# Phase 1 — Kickoff

## In scope

| Nhóm | Phạm vi |
| --- | --- |
| Functional | Course/Lesson/Enrollment/Progress, Student query, Media upload/content/usage, Notification/batch và Scheduler data boundary. |
| Technical | ASP.NET Core, Clean Architecture, YARP, MySQL, MinIO (Media ownership), RabbitMQ/MassTransit, Docker Compose, Worker, response/error/correlation và NUnit/Testcontainers. |
| Performance | Kế hoạch benchmark batch, CSV streaming, N+1, index/query plan, pagination và API; chưa là claim runtime. |

## Out of scope

Payment, chat, livestream, exam, certificate, recommendation AI, production-grade
Kubernetes và multi-region deployment. Bất cứ feature nào xuất hiện ở source sau
này phải được đánh giá lại trước khi giữ trong Out of Scope.

## Constraints & Dependencies

- Solo developer, timeline mini-project, local resource và demo-oriented.
- MySQL 8.4, MinIO, RabbitMQ, Docker Engine là runtime/development dependency.
- Notification phụ thuộc Student query và Media usage qua boundary; không có FK/query xuyên database.
- Benchmark cần cùng máy, dataset, cấu hình, warm-up (nếu dùng) và nhiều lần chạy.

Chi tiết: [assumptions](registers/assumptions.md) và [risks](registers/risks.md).
