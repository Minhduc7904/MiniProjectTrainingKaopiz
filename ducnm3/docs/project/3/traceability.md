# Requirement Traceability — Phase 3

| Requirements mapping | Function | Rule / AC / NFR | Input/evidence | Phase sau |
| --- | --- | --- | --- | --- |
| Docker | F01–F18 | NFR-ENV-01 | `docker-compose.yml`, `docs/development/docker.md` | Compose/config testcase và startup evidence |
| MinIO / Object Storage | F09–F11 | BR-MEDIA-01, BR-MEDIA-02, BR-MEDIA-03, BR-MEDIA-04; AC F09–F11 | `docs/business-flows/media/` | Storage/security design và testcase |
| Batch Job | F14–F16 | BR-NOTI-02, BR-NOTI-03; AC F14–F16; NFR-BATCH-02 | `docs/business-flows/notifications/bulk-notification.md` | Worker/message design và testcase |
| Batch – Retry | F16–F17 | BR-NOTI-03, BR-NOTI-04; AC F16; NFR-REL-01, NFR-REL-02 | `docs/business-flows/notifications/bulk-notification.md` | Retry/idempotency testcase |
| Batch – Performance | F14–F17 | NFR-BATCH-01, NFR-BATCH-02 | `docs/development/performance.md` | Benchmark 3k/10k/100k |
| CSV Export | F06 | AC F06; NFR-CSV-01 | `docs/api/course-service/endpoints/get-courses-export.md` | Export design và testcase |
| CSV – Performance | F06 | NFR-CSV-01 | `docs/development/performance.md` | Baseline/streaming benchmark |
| API – N+1 | F03 | NFR-QUERY-01 | `docs/development/performance.md` | Query design và query-count benchmark |
| API – Index | F03/F06 | NFR-INDEX-01 | `docs/development/performance.md` | Index experiment 10k/100k/1M |
| API – Query Plan | F03/F06 | NFR-PLAN-01 | `docs/development/performance.md` | `EXPLAIN` evidence trước/sau |
| API – Pagination | F03/F07 | BR-STUDENT-02; NFR-PAGING-01 | `docs/api/student-service/endpoints/get-students.md`, `docs/development/performance.md` | Offset/cursor performance testcase |
| API – Performance | F03/F07 | NFR-API-01 | `docs/development/performance.md` | Benchmark 10k/100k trước/sau optimization |
| API – Error Handling | F01–F14, F17–F18 | Error cases từng function; NFR-ERROR-01, NFR-OBS-01 | `docs/api/shared/error-format.md`, `docs/api/shared/error-handling-observability.md` | Error contract và happy/boundary/negative testcase |

## Traceability bổ sung từ scope Phase 0–2

| Scope | Function | NFR | Input/evidence | Phase sau |
| --- | --- | --- | --- | --- |
| Authorization và không lộ storage location | F01–F14, F17–F18 | NFR-SEC-01 | `docs/api/shared/error-handling-observability.md`, `docs/business-flows/` | Security/negative testcase |
| Database-per-service và ownership | F01–F18 | NFR-MAINT-01 | `docs/architecture/`, `docs/database/` | Architecture/integration review |

## Quy tắc chuyển tiếp sang Phase 4

Mỗi Function ID và NFR ID trong bảng này phải được tham chiếu bởi Basic Design
và testcase tương ứng. Nếu requirement bị thay đổi, cập nhật đồng thời Function
List, spec, NFR/AC liên quan và dòng traceability trước khi xin lại sign-off G2.
