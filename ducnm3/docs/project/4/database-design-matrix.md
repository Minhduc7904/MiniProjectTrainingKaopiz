# Database Design Matrix

## Ownership

| Service | Database | Tables chính | Source of truth |
| --- | --- | --- | --- |
| Course | `lms_course_db` | `courses`, `lessons`, `enrollments`, `lesson_progresses` | [Course data model](../../database/course-service/data-model.md) |
| Student | `lms_student_db` | `students` | [Student data model](../../database/student-service/data-model.md) |
| Media | `lms_media_db` | `media_objects`, `media_derivation_jobs`, `media_usages`, MassTransit tables | [Media data model](../../database/media-service/data-model.md) |
| Notification | `lms_notification_db` | `notification_batches`, `notification_batch_items`, `notifications`, MassTransit tables | [Notification data model](../../database/notification-service/data-model.md) |
| Scheduler | `lms_scheduler_db` | `background_jobs`, `background_job_runs` | [Scheduler data model](../../database/scheduler-service/data-model.md) |

Không tạo foreign key hoặc query xuyên database. Student ID, owner ID và
notification ID ở service khác chỉ là logical reference.

## Function/data mapping

| Function | Read | Write | Constraint/index bảo vệ | Transaction/consistency |
| --- | --- | --- | --- | --- |
| F01 | `courses`, `lessons` khi publish | `courses` | status check, course indexes | Một Course DB transaction; publish chỉ khi invariant đạt. |
| F02 | `courses`, `lessons` | `lessons` | `uq_lessons_course_id_display_order` | Kiểm tra parent/order và write cùng transaction. |
| F03 | `courses`, `lessons`, `lesson_progresses` | Không | course filter/cursor, lesson composite key | Read-only; query shape phải tránh N+1. |
| F04 | `courses`, `enrollments`; Student qua HTTP/JWT | `enrollments` | `uq_enrollments_course_id_student_id` | Verify actor/status trước transaction; unique key xử lý race/replay. |
| F05 | `lessons`, `enrollments`, `lesson_progresses` | `lesson_progresses` | `uq_lesson_progresses_lesson_id_student_id`, range check | Upsert một transaction; `completed_at` nhất quán với 100%. |
| F06 | `courses` | Không | index theo filter/sort được benchmark | Stream/chunk, không giữ toàn dataset/CSV string. |
| F07–F08 | `students` | Không | status/sort/paging indexes | Read-only và stable ordering. |
| F09 | `media_objects`, `media_derivation_jobs` | cùng tables | checksum/state/job uniqueness | DB state tách MinIO side effect; compensation khi upload lỗi. |
| F10 | `media_objects`, `media_usages` | Không | lookup/active usage indexes | Chỉ Media Service đọc MinIO. |
| F11 | `media_objects`, `media_usages` | `media_usages` | unique active usage/command idempotency | Replace active usage hoặc worker upsert trong một transaction. |
| F12 | `notifications` | `notifications`, Outbox | recipient/status indexes | Notification + media command outbox atomic. |
| F13 | `notifications` | read status/`read_at` | recipient/status/cursor index | Ownership filter trong query/update; read lặp là no-op. |
| F14 | `notification_batches` | batch + Outbox | batch state/created index | Batch `PENDING` + snapshot command atomic. |
| F15 | batch/items | items, batch total/status, Outbox | unique `(batch_id, student_id)` | Persist theo page; dispatch command sau snapshot durable. |
| F16 | items/batch | notifications, item/counter, Outbox | lease token/index, notification uniqueness | Claim transaction ngắn; commit kết quả theo bounded chunk. |
| F17 | batch/items | Không | batch ID và failed-item cursor index | Read-only, counter/status nhất quán. |
| F18 | jobs/runs | job run/status/summary | unique `(job_id, idempotency_key)` | Scheduler run + command handoff cần Outbox hoặc consistency strategy trước implement. |

## Trạng thái schema

Phase 4 không tạo migration. Các table/constraint trên đã có trong database docs;
riêng optimistic concurrency cho Course PATCH và durable handoff Scheduler còn
phụ thuộc quyết định ở [Q&A](open-questions.md). Nếu quyết định làm thay đổi
schema, Phase 5 phải tạo migration version mới và integration test bằng
Testcontainers; không sửa migration đã được ghi trong `schema_migrations`.

