# Phase 0 — Project Initiation

## Background & Objective

Mini LMS là sandbox để demo Course, Student, Media và Notification đồng thời
chứng minh boundary microservice, storage, messaging, migration và test. Đây
không phải LMS production đầy đủ.

| Nhóm | Objective đo được |
| --- | --- |
| Business/demo | Dựng được stack và trình bày luồng Media/Notification có evidence. |
| Technical | Chứng minh Clean Architecture, database-per-service, YARP, MySQL, MinIO, RabbitMQ/MassTransit và Worker. |
| Performance | Measure-and-compare batch 3k/10k/100k, CSV 100k+, N+1, index, query plan, pagination và API; target số cụ thể chưa được lead quy định. |

## Success criteria

- Docker Compose dựng được Gateway, năm API service, Media/Notification/Scheduler Worker, MySQL, MinIO và RabbitMQ theo source hiện tại.
- Demo có architecture, script và số đo thật khi benchmark được triển khai.
- Không công bố benchmark, CSV 100k+, N+1/index optimization hay Scheduler execution là hoàn tất khi chưa có evidence.

## Stakeholders

| Vai trò | Trách nhiệm |
| --- | --- |
| Developer/Owner | Thực hiện, tự kiểm thử, trình bày và duy trì artifacts. |
| Lead | Review direction, scope và kết quả benchmark/demo. |
| Backend review council/Mentor | Feedback kỹ thuật khi thực tế có người tham gia. |
