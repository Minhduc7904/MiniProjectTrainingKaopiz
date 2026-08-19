# Notification Service Tasks — Phase 5

## P5-14 — F12 Hoàn thiện tạo thông báo đơn

| Thuộc tính | Giá trị |
| --- | --- |
| Est | 5 giờ |
| Ticket | `Chưa tạo` |
| Loại | Existing/hardening |
| Dependency | P5-04, P5-13 |
| Baseline | F12, TC-NOTI-F12-001..003 |

**Phạm vi:** Audit create notification, recipient/creator identity, Markdown
media reference và atomic Notification + Outbox; Media usage chỉ được tạo sau
khi Notification thành công.

**Code/test chính:** Notification create endpoint/handler/repository,
`NotificationMediaReferenceExtractor`, Outbox/message contracts và unit/
integration tests.

**Hoàn thành khi:** validation/recipient/media failure không để dữ liệu hoặc
message nửa vời; success có persistent notification và idempotent media command;
functional testcase và error/correlation evidence pass.

## P5-15 — F13 Triển khai hộp thư đến và đánh dấu đã đọc

| Thuộc tính | Giá trị |
| --- | --- |
| Est | 8 giờ |
| Ticket | `Chưa tạo` |
| Loại | Planned |
| Dependency | P5-14 |
| Baseline | F13, TC-NOTI-F13-001..003 |

**Phạm vi:** Triển khai GET inbox/detail, PATCH read và POST read-all cho
current Student; filter ownership trong query/update; stable paging; đọc lặp là
no-op và không sửa notification của user khác.

**Code dự kiến:** contracts/endpoints Notification, Application Inbox/Read
features, repository EF, migration version mới nếu thiếu index/column, unit/
component/integration test.

**Hoàn thành khi:** inbox/detail/read/read-all đúng contract; unauthorized,
cross-owner, not-found và replay được test; DB state/read timestamp/counter đúng;
API docs/Postman và `docs/tests` có evidence.

## P5-16 — F14 Hoàn thiện tạo Notification batch

| Thuộc tính | Giá trị |
| --- | --- |
| Est | 16 giờ |
| Ticket | `ERBUL26-2978` |
| Loại | Existing/hardening |
| Dependency | P5-03, P5-14 |
| Baseline | F14, TC-NOTI-F14-001..003 |

**Phạm vi:** Hoàn thiện POST batch với `requestedCount` null/`1..100000`, snapshot
có giới hạn; thêm GET list offset, duration, trang FE quản lý/progress theo URL và
POST retry tạo batch con chỉ từ item `FAILED`. Tạo batch trả `202 Accepted` cùng
canonical `Location`, không hiểu accepted là dispatch xong.

**Hoàn thành khi:** invalid request không tạo batch/message; requested count không
tải quá giới hạn; list/duration đúng; retry không gửi lại SUCCESS và replay cùng
nguồn trả cùng child; backend/frontend/unit/component/integration test pass.

## P5-17 — F15 Hoàn thiện snapshot recipient batch

| Thuộc tính | Giá trị |
| --- | --- |
| Est | 6 giờ |
| Ticket | `Chưa tạo` |
| Loại | Existing/hardening |
| Dependency | P5-16 |
| Baseline | F15, TC-NOTI-F15-001..003, NFR-REL-01 |

**Phạm vi:** Audit snapshot consumer, cursor paging Student, bounded insert,
unique `(batch_id, student_id)`, counter/status và durable dispatch command sau
khi snapshot hoàn tất.

**Hoàn thành khi:** redelivery/page retry không nhân đôi recipient; partial
failure có thể resume; empty recipient kết thúc đúng state; integration test
MySQL/RabbitMQ và testcase F15 pass.

## P5-18 — F16 Hoàn thiện dispatch, retry và hiệu năng batch

| Thuộc tính | Giá trị |
| --- | --- |
| Est | 10 giờ |
| Ticket | `Chưa tạo` |
| Loại | Existing/hardening |
| Dependency | P5-17 |
| Baseline | F16, TC-NOTI-F16-001..004, NFR-BATCH-01/02, NFR-REL-01/02 |

**Phạm vi:** Audit bounded claim/lease, gửi notification, retry đúng một lần,
terminal failure, counter/status và media usage command; item `SUCCESS` không
được xử lý lại.

**Code/test chính:** Dispatch handler/consumer, batch repository, sender,
Outbox/message contracts, Notification unit/integration tests và benchmark
scripts/evidence.

**Hoàn thành khi:** concurrent worker không double-send; lease timeout/recovery,
retry một lần và terminal error có safe code; benchmark 3k/10k/100k ghi total
time, throughput, peak memory và failure count; không load toàn bộ recipient vào
memory; testcase F16 và reliability test pass.

## P5-19 — F17 Hoàn thiện theo dõi Notification batch

| Thuộc tính | Giá trị |
| --- | --- |
| Est | 5 giờ |
| Ticket | `Chưa tạo` |
| Loại | Existing/hardening |
| Dependency | P5-16, P5-18 |
| Baseline | F17, TC-NOTI-F17-001..003, NFR-OBS-01 |

**Phạm vi:** Audit batch detail và failed-items cursor endpoint; bảo đảm
counter/status nhất quán với items, stable paging và error detail an toàn để
trace mà không lộ payload nhạy cảm.

**Hoàn thành khi:** running/completed/partial-failed state phản ánh DB; failed
items page không trùng/mất; correlation/batch ID truy vết được qua log; endpoint
tests và testcase F17 pass.
