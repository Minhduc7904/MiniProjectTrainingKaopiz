# Giao tiếp giữa các service

| Nhu cầu | Transport | Quy tắc |
| --- | --- | --- |
| QUERY cần response | HTTP typed client | Retry chỉ request idempotent; forward correlation ID. |
| COMMAND một owner | RabbitMQ Send | Queue thuộc owner; consumer idempotent. |
| EVENT nhiều subscriber | RabbitMQ Publish | Mỗi subscriber có queue riêng. |

RabbitMQ delivery là at-least-once. Consumer phải idempotent; retry, prefetch, concurrency và error queue được cấu hình tập trung. Media và Notification đã cấu hình Entity Framework Outbox. Không dùng transport để che giấu truy vấn chéo database.

