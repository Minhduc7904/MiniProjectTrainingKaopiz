# Media Service Tasks — Phase 5

## P5-11 — F09 Hoàn thiện upload và xử lý Media

| Thuộc tính | Giá trị |
| --- | --- |
| Est | 6 giờ |
| Ticket | `Chưa tạo` |
| Loại | Existing/hardening |
| Dependency | P5-02 |
| Baseline | F09, TC-MEDIA-F09-001..003 |

**Phạm vi:** Audit upload API, MinIO stream, metadata lifecycle
`PENDING → READY/FAILED`, thumbnail command/worker và compensation khi DB,
storage hoặc message thất bại.

**Code/test chính:** Media API Upload endpoint, Application Upload/Derivations,
Infrastructure MinIO/Persistence, Worker và Unit/Integration tests hiện có.

**Hoàn thành khi:** validation không tạo side effect; object/metadata không bị
mồ côi trong các failure path; thumbnail retry idempotent; không lộ object key;
testcase F09 pass với MySQL/MinIO/RabbitMQ boundary cần thiết.

## P5-12 — F10 Hoàn thiện truy cập Media an toàn

| Thuộc tính | Giá trị |
| --- | --- |
| Est | 5 giờ |
| Ticket | `Chưa tạo` |
| Loại | Existing/hardening |
| Dependency | P5-11 |
| Baseline | F10, TC-MEDIA-F10-001..003, NFR-SEC-01 |

**Phạm vi:** Audit metadata/content/thumbnail/usage URL endpoints, ownership và
authorization; binary response không dùng JSON envelope; response metadata/URL
không lộ bucket/object key.

**Code/test chính:** Media GetContent/GetUrls endpoints và handlers, URL
provider, MinIO adapter, component/unit/integration tests.

**Hoàn thành khi:** owner/allowed usage truy cập được; unauthorized/not-found
không lộ resource; range/content type nếu contract yêu cầu hoạt động đúng;
security negative testcase và storage integration test pass.

## P5-13 — F11 Hoàn thiện quản lý Media usage

| Thuộc tính | Giá trị |
| --- | --- |
| Est | 5 giờ |
| Ticket | `Chưa tạo` |
| Loại | Existing/hardening |
| Dependency | P5-11, P5-12 |
| Baseline | F11, TC-MEDIA-F11-001..003, NFR-REL-01 |

**Phạm vi:** Audit create/get/delete usage và consumer đăng ký usage từ
Notification; bảo vệ unique active usage, soft-delete/reuse và message
idempotency trong transaction.

**Code/test chính:** Media Usages features/endpoints, EF repository,
`RegisterNotificationMediaUsageConsumer` và unit/component/integration tests.

**Hoàn thành khi:** duplicate/replay không tạo active usage thứ hai; replace và
delete giữ ownership; message redelivery an toàn; persistent state và URL side
effect được xác minh trong testcase F11.
