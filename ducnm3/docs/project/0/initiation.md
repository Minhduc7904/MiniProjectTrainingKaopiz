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

Các nhóm performance ở đây là objective đo lường, không phải claim đã tối ưu.
Dataset, baseline và metric cụ thể được khóa ở Phase 2 rồi chuyển thành NFR tại
Phase 3.

## Success criteria

- Docker Compose dựng được Gateway, năm API service, Media/Notification/Scheduler Worker, MySQL, MinIO và RabbitMQ theo source hiện tại.
- Danh sách 13 yêu cầu của Lead được trace riêng tới scope, planning và output
  Phase sau; không gộp thành một dòng “Performance” khi review.
- Demo có architecture, script và số đo thật khi benchmark được triển khai.
- Không công bố benchmark, CSV 100k+, N+1/index optimization hay Scheduler execution là hoàn tất khi chưa có evidence.

## Stakeholders

| Vai trò | Trách nhiệm |
| --- | --- |
| Developer/Owner | Thực hiện, tự kiểm thử, trình bày và duy trì artifacts. |
| Lead | Review direction, scope và kết quả benchmark/demo. |
| Backend review council/Mentor | Feedback kỹ thuật khi thực tế có người tham gia. |
