# Scheduler Service Tasks — Phase 5

## P5-20 — F18 Triển khai Scheduler dọn dẹp Media

| Thuộc tính | Giá trị |
| --- | --- |
| Est | 12 giờ |
| Ticket | `Chưa tạo` |
| Loại | Planned |
| Dependency | P5-11, P5-12, P5-13 |
| Baseline | F18, TC-SCHED-F18-001..004, NFR-REL-01, NFR-OBS-01, Q4-04 |

**Phạm vi:** Triển khai Scheduler Worker tạo job run/idempotency key, durable
handoff `CleanupStaleMediaV1`; Media Worker là owner tìm/xóa object đủ điều kiện
và phát `MediaCleanupCompletedV1` hoặc `MediaCleanupFailedV1`; Scheduler cập
nhật terminal status/summary.

**Code dự kiến:**

- Scheduler Application feature và repository/job runner trong
  `backend/Services/Scheduler/`.
- Message contracts trong project Contracts/Applications phù hợp ownership.
- Scheduler Worker consumer/publisher và Media Worker cleanup consumer.
- Migration version mới cho Outbox/Inbox hoặc schema cần thiết; không sửa
  `V001` đã áp dụng.
- Scheduler/Media unit, messaging integration và Testcontainers integration
  tests.

**Hoàn thành khi:**

- Một schedule/run chỉ tạo một logical cleanup theo idempotency key; redelivery
  không xóa/đếm hai lần.
- Media chỉ xóa object stale, không có active usage; failure lưu safe error và
  có thể retry/recover theo design.
- Scheduler không truy cập Media DB/MinIO trực tiếp; command/event có
  correlation và trace qua log.
- Testcase F18, migration test và RabbitMQ/MySQL/MinIO integration flow pass;
  docs message/database/runbook được đồng bộ.
