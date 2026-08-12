# Mini Project LMS Microservices — Kế hoạch triển khai 5 ngày

## 1. Mục tiêu dự án

Xây dựng một **LMS mini** theo kiến trúc **Microservices + Clean Architecture**, đủ nhỏ để hoàn thành và demo trong **5 ngày**, nhưng vẫn cover được các bài toán backend mà lead yêu cầu:

- Docker / Docker Compose
- MySQL
- MinIO / Object Storage
- Batch Job
- Retry + Failure Tracking + Idempotency
- Batch Performance
- CSV Export 100k+ records
- CSV Streaming / Chunking
- N+1 Query
- Database Index
- `EXPLAIN` / `EXPLAIN ANALYZE`
- Offset Pagination vs Cursor Pagination
- API Benchmark
- Error Handling
- UML / Architecture Diagram
- Có số liệu **Before / After Optimization**

> Mục tiêu của mini project không phải làm một LMS đầy đủ tính năng.
> Mục tiêu là tạo đủ "đất diễn" để chứng minh hiểu kiến trúc, database, performance, batch processing và reliability.

---
# 2. Scope chức năng

## 2.1. Actor

### Admin

- Tạo/sửa/xóa Course.
- Tạo Lesson.
- Upload và quản lý nhiều media: thumbnail, image, video, audio, tài liệu.
- Xem danh sách Course.
- Export Course CSV.
- Gửi thông báo cho một học viên.
- Gửi thông báo hàng loạt cho học viên.
- Xem trạng thái batch notification.
- Xem các item gửi thất bại.

### Student

- Xem Course đã enroll.
- Xem Lesson.
- Cập nhật Lesson Progress.
- Xem inbox notification của chính mình và đánh dấu đã đọc.

---
# 3. Những chức năng KHÔNG làm trong 5 ngày

Để tránh scope nổ:

- Payment
- Exam realtime
- Chat
- Permission chi tiết
- Refresh token phức tạp
- Email template editor
- Video transcoding
- Search engine Elasticsearch
- Kafka/RabbitMQ
- Kubernetes
- Distributed tracing phức tạp
- CQRS/Event Sourcing đầy đủ

Authentication có thể dùng JWT đơn giản nếu cần demo.

---
