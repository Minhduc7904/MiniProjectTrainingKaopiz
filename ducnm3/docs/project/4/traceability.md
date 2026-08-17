# Traceability — Phase 4

## Functional traceability

| Function | Requirement | Basic Design | Testcase |
| --- | --- | --- | --- |
| F01 | [Course spec](../3/course-spec.md#f01--quản-lý-khóa-học) | [API](api-design-matrix.md#course), [DB](database-design-matrix.md#functiondata-mapping), Course management flow | TC-COURSE-F01-001..003 |
| F02 | [Course spec](../3/course-spec.md#f02--quản-lý-bài-học) | API/DB matrix, Course management flow | TC-COURSE-F02-001..003 |
| F03 | [Course spec](../3/course-spec.md#f03--tra-cứu-khóa-học) | API/DB matrix, Course query docs | TC-COURSE-F03-001..003; TC-NFR-QUERY-001; TC-NFR-INDEX-001; TC-NFR-PLAN-001; TC-NFR-PAGING-001; TC-NFR-API-001 |
| F04 | [Course spec](../3/course-spec.md#f04--ghi-danh-khóa-học) | API/DB matrix, enrollment flow, Q4-02/Q4-03 | TC-COURSE-F04-001..003 |
| F05 | [Course spec](../3/course-spec.md#f05--cập-nhật-tiến-độ-bài-học) | API/DB matrix, learning progress flow | TC-COURSE-F05-001..003 |
| F06 | [Course spec](../3/course-spec.md#f06--xuất-danh-sách-khóa-học-csv) | API/DB matrix, CSV contract | TC-COURSE-F06-001..003; TC-NFR-CSV-001 |
| F07 | [Student spec](../3/student-spec.md#f07--tra-cứu-danh-sách-học-viên) | API/DB matrix, Student list flow | TC-STUDENT-F07-001..003; TC-NFR-PAGING-001; TC-NFR-API-001 |
| F08 | [Student spec](../3/student-spec.md#f08--tra-cứu-chi-tiết-học-viên) | API/DB matrix, Student detail flow | TC-STUDENT-F08-001..003 |
| F09 | [Media spec](../3/media-spec.md#f09--upload-và-xử-lý-media) | API/DB/message matrix, upload flow | TC-MEDIA-F09-001..003 |
| F10 | [Media spec](../3/media-spec.md#f10--truy-cập-media-an-toàn) | API/DB matrix, content/usage URL flows | TC-MEDIA-F10-001..003; TC-NFR-SEC-001 |
| F11 | [Media spec](../3/media-spec.md#f11--quản-lý-media-usage) | API/DB/message matrix, usage flow | TC-MEDIA-F11-001..003; TC-NFR-REL-001 |
| F12 | [Notification spec](../3/notification-spec.md#f12--tạo-thông-báo-đơn) | API/DB/message matrix, single notification flow | TC-NOTI-F12-001..003 |
| F13 | [Notification spec](../3/notification-spec.md#f13--quản-lý-hộp-thư-đến) | API/DB matrix, inbox flow | TC-NOTI-F13-001..003 |
| F14 | [Notification spec](../3/notification-spec.md#f14--tạo-notification-batch) | API/DB/message matrix, batch flow | TC-NOTI-F14-001..003; TC-NFR-BATCH-001; TC-NFR-BATCH-002 |
| F15 | [Notification spec](../3/notification-spec.md#f15--snapshot-recipient-batch) | DB/message matrix, batch flow | TC-NOTI-F15-001..003; TC-NFR-REL-001 |
| F16 | [Notification spec](../3/notification-spec.md#f16--dispatch-và-retry-notification-batch) | DB/message matrix, lease/retry design | TC-NOTI-F16-001..004; TC-NFR-BATCH-001; TC-NFR-BATCH-002; TC-NFR-REL-001; TC-NFR-REL-002 |
| F17 | [Notification spec](../3/notification-spec.md#f17--theo-dõi-notification-batch) | API/DB matrix, batch query docs | TC-NOTI-F17-001..003; TC-NFR-OBS-001 |
| F18 | [Scheduler spec](../3/scheduler-spec.md#f18--chạy-dọn-dẹp-media-theo-lịch) | DB matrix, [messaging](messaging-worker-design.md#scheduler-cleanup-contract-đề-xuất), cleanup flow | TC-SCHED-F18-001..004; TC-NFR-REL-001; TC-NFR-OBS-001 |

Các testcase functional được định nghĩa tại:

- [Course](testcases/course-testcases.md)
- [Student](testcases/student-testcases.md)
- [Media](testcases/media-testcases.md)
- [Notification](testcases/notification-testcases.md)
- [Scheduler](testcases/scheduler-testcases.md)

## NFR traceability

| NFR | Lead requirement | Design | Testcase |
| --- | --- | --- | --- |
| NFR-ENV-01 | Docker | Design baseline + Compose docs | TC-NFR-ENV-001 |
| NFR-BATCH-01, NFR-BATCH-02 | Batch Job/Performance | Notification messaging/worker design | TC-NFR-BATCH-001, TC-NFR-BATCH-002 |
| NFR-CSV-01 | CSV Export/Performance | F06 API/DB design | TC-NFR-CSV-001 |
| NFR-QUERY-01 | API N+1 | F03 query/data design | TC-NFR-QUERY-001 |
| NFR-INDEX-01 | API Index | Course index design | TC-NFR-INDEX-001 |
| NFR-PLAN-01 | API Query Plan | Course query experiment design | TC-NFR-PLAN-001 |
| NFR-PAGING-01 | API Pagination | F03/F07 API design | TC-NFR-PAGING-001 |
| NFR-API-01 | API Performance | F03/F07 benchmark design | TC-NFR-API-001 |
| NFR-REL-01, NFR-REL-02 | Batch Retry/reliability | Message idempotency + lease design | TC-NFR-REL-001, TC-NFR-REL-002 |
| NFR-SEC-01 | Authorization/storage secrecy | API error/access baseline | TC-NFR-SEC-001 |
| NFR-OBS-01 | Traceability/logging | Failure/recovery design | TC-NFR-OBS-001 |
| NFR-MAINT-01 | Database/service ownership | Design/database baseline | TC-NFR-MAINT-001 |
| NFR-ERROR-01 | API Error Handling | API error baseline | TC-NFR-ERROR-001 |

NFR testcase chi tiết nằm tại
[non-functional-testcases.md](testcases/non-functional-testcases.md).

## Quy tắc chuyển sang Phase 5

1. Đóng Q4-01..Q4-04 và đồng bộ mọi artefact chịu ảnh hưởng.
2. Mỗi implementation ticket chọn rõ Function ID, testcase và NFR liên quan.
3. Endpoint mới phải bổ sung API doc/business-flow 1:1 và Postman trong cùng
   implementation change.
4. Schema change phải có migration version mới và integration test.
5. Khi test được implement, đổi trạng thái `Designed/Planned` sang `Existing`
   chỉ sau khi `docs/tests` có evidence tương ứng.
