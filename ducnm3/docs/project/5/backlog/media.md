# Media Service Tasks — Phase 5

## P5-11 — F09 Triển khai direct upload và trạng thái draft ban đầu

| Thuộc tính | Giá trị |
| --- | --- |
| Est | 16 giờ |
| Ticket | `ERBUL26-2914` |
| Loại | Extension |
| Dependency | P5-02 |
| Baseline | F09, TC-MEDIA-F09-001..003 |

**Phạm vi:** Giữ nguyên multipart/thumbnail baseline đã hoàn thành; thêm browser
direct upload qua presigned POST, progress không persist, idempotent complete và
initial draft state/backfill V005. P5-13 xử lý draft theo usage; P5-20 xử lý
cleanup sau reference recheck.

**Code/test chính:** upload-intent/complete endpoints, Application direct-upload,
MinIO signing/promotion, persistence V005, `/media/upload-direct`, Unit/
Component/Integration/frontend tests và canonical docs.

**Hoàn thành khi:** browser upload không đi qua API; policy exact size/type/key/
checksum hết hạn 15 phút; complete retry an toàn và không nhân thumbnail work;
multipart/direct media đều draft; active usage legacy được backfill non-draft;
signed material không lộ trong log/normal API.

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
