# Testcase — Notification

Runtime/test evidence tham khảo:
[Notification unit](../../../tests/notification-service/unit.md) và
[integration](../../../tests/notification-service/integration.md).

| TC ID | Trace | Level/status | Setup / Test data | Action | Expected |
| --- | --- | --- | --- | --- | --- |
| TC-NOTI-F12-001 | F12 AC, BR-NOTI-01 | Unit + integration / Existing một phần | Recipient ACTIVE; payload không media | Tạo notification đơn | Một notification `SINGLE/UNREAD`; không có media command. |
| TC-NOTI-F12-002 | F12 alternative, BR-NOTI-05 | Unit / Existing | Markdown chứa media READY hợp lệ | Tạo notification | Notification + outbox command atomic; usage chỉ xuất hiện sau consumer thành công. |
| TC-NOTI-F12-003 | F12 negative | Unit + component / Existing một phần | Recipient inactive, media sai, actor không quyền | POST | Error an toàn; không có notification/outbox/usage dang dở. |
| TC-NOTI-F13-001 | F13 AC | Component + integration / Planned | Inbox có UNREAD thuộc current Student | List/detail/read | Chỉ thấy item của mình; read chuyển `READ` và đặt `read_at`. |
| TC-NOTI-F13-002 | F13 alternative | Component + integration / Planned | Inbox rỗng hoặc item đã READ | List/read/read-all | List rỗng hợp lệ; thao tác lặp là no-op, không đổi `read_at` ngoài policy. |
| TC-NOTI-F13-003 | F13 negative | Component / Planned | ID thuộc Student khác hoặc identity thiếu | Truy cập/read | `401/403/404` theo contract; dữ liệu owner không đổi. |
| TC-NOTI-F14-001 | F14 AC, BR-NOTI-02 | Unit + component / Existing | 3k ACTIVE; scope `ALL_STUDENTS` | POST batch | `202` + `Location`; chỉ batch `PENDING` + snapshot outbox; HTTP không tạo items/inbox. |
| TC-NOTI-F14-002 | F14 boundary | Unit / Existing một phần | 1/3k/10k/100k recipient | Tạo batch | API behavior không phụ thuộc tổng recipient; xử lý chuyển sang background. |
| TC-NOTI-F14-003 | F14 negative | Unit + component / Existing | Scope lạ, title rỗng, outbox persistence lỗi | POST | `400` hoặc dependency error; không accepted nếu durable handoff không bảo đảm. |
| TC-NOTI-F15-001 | F15 AC, BR-NOTI-03 | Unit / Existing | Student Service trả nhiều page ACTIVE | Snapshot | Mỗi student đúng một item, total đúng, batch `SNAPSHOT_READY`, dispatch được enqueue. |
| TC-NOTI-F15-002 | F15 alternative | Unit + integration / Existing một phần | Snapshot command redelivery, page cuối ngắn | Consume lặp | Upsert không trùng `(batch, student)`; total và dispatch slot đúng. |
| TC-NOTI-F15-003 | F15 negative/boundary | Unit / Existing một phần | 0 recipient hoặc page/dependency lỗi | Snapshot | Không dispatch nhầm; batch `FAILED`/safe trace theo contract. |
| TC-NOTI-F16-001 | F16 AC, BR-NOTI-03 | Unit + integration / Existing một phần | Chunk item PENDING; sender thành công | Dispatch | Notifications/item/counter commit đúng chunk; item `SUCCESS`; batch hoàn thành phù hợp. |
| TC-NOTI-F16-002 | F16 retry, BR-NOTI-04 | Unit / Existing | Sender lỗi lần đầu | Dispatch | Item `RETRY`, retry count 1, command tiếp theo; chưa đánh dấu terminal failure. |
| TC-NOTI-F16-003 | F16 negative, BR-NOTI-04 | Unit / Existing | Cùng item lỗi lần hai | Dispatch | Item `FAILED`, safe error; batch `PARTIAL_FAILED` hoặc `FAILED` theo counter. |
| TC-NOTI-F16-004 | F16 alternative, NFR-REL-01/02 | Integration / Existing một phần | Hai worker, lease expired/stale token, redelivery | Claim/ghi kết quả | Claim disjoint; chỉ token hợp lệ update; không tạo inbox/usage trùng. |
| TC-NOTI-F17-001 | F17 AC | Component / Existing | Batch COMPLETED/PARTIAL_FAILED | GET summary/failed-items | Status và counter nhất quán; failed item page đúng batch. |
| TC-NOTI-F17-002 | F17 alternative | Component / Existing một phần | Batch đang chạy hoặc không có failed item | GET | Trả trạng thái hiện tại/list rỗng với cursor metadata đúng. |
| TC-NOTI-F17-003 | F17 negative | Component / Existing một phần | batchId sai, cursor sai, actor không quyền | GET | `400/403/404`; không trả batch khác. |

## Kiểm tra side effect bắt buộc

- Query Notification DB để đếm batch, item và notification thực tế.
- Kiểm tra Outbox/message count cho snapshot, dispatch và media usage.
- Redelivery phải giữ nguyên business row count.
- Error assertion không dựa vào raw exception hoặc provider-specific SQL.

