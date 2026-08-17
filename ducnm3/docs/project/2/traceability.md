# Lead-level Requirement Traceability

## Đối chiếu 13 yêu cầu Lead

| Requirements mapping | Scope/planning | Evidence đầu vào | Status | Output Phase 3 |
| --- | --- | --- | --- | --- |
| Docker | Technical scope; environment strategy | `docker-compose.yml`, `docs/development/docker.md` | Current; cần verify config/case | NFR môi trường/container |
| MinIO / Object Storage | Media upload/content/usage; Media ownership | `docs/business-flows/media/`, `docs/development/minio.md` | Current | F09–F11 và BR/NFR storage |
| Batch Job | Notification batch chạy background theo page/chunk | `docs/business-flows/notifications/bulk-notification.md` | Current | F14–F16 và AC batch |
| Batch – Retry | Retry đúng một lần, failure trace và idempotency | `docs/business-flows/notifications/bulk-notification.md` | Current; cần negative testcase | F16–F17 và BR/NFR reliability |
| Batch – Performance | 3k/10k/100k, time/memory/throughput | `docs/development/performance.md` | Planned; chưa có số đo | NFR batch và benchmark plan |
| CSV Export | Export danh sách Course 100k+ | `docs/api/course-service/endpoints/get-courses-export.md` | Planned; chưa có runtime endpoint | F06 và AC export |
| CSV – Performance | SELECT ALL/build string so với batch/stream | `docs/development/performance.md` | Planned; chưa có số đo | NFR CSV |
| API – N+1 | Course → Lessons → Progress; query count trước/sau | `docs/development/performance.md` | Planned | NFR query/N+1 |
| API – Index | 10k/100k/1M, đánh giá lợi ích/trade-off | `docs/development/performance.md` | Planned | NFR index |
| API – Query Plan | `EXPLAIN`/`EXPLAIN ANALYZE`, scan/index/bottleneck | `docs/development/performance.md` | Planned | NFR query plan |
| API – Pagination | Offset tăng và cursor/keyset khi phù hợp | Student list evidence, `docs/development/performance.md` | Partial current; experiment Planned | F03/F07 và NFR pagination |
| API – Performance | API 10k/100k, response time trước/sau | `docs/development/performance.md` | Planned; chưa có số đo | NFR API performance |
| API – Error Handling | Validation, not found, database/dependency error, trace ID | `docs/api/shared/error-format.md`, `docs/api/shared/error-handling-observability.md` | Foundation current; endpoint verification Planned | Error cases và NFR error/observability |

`Current` chỉ dùng khi có runtime/source evidence. `Planned` là requirement hoặc
kế hoạch đo; không được dùng làm claim capability/optimization đã hoàn tất.

## Traceability bổ sung cho delivery

| Need cấp lead | Evidence/artefact hiện có | Phase tiếp theo |
| --- | --- | --- |
| Service boundary và ownership | `docs/architecture/`, `docs/database/` | Requirements/Design xác nhận AC. |
| Git/workspace | `rules/git.md`, `docs/guide/GIT_GUIDE.md` | Development thực thi ticket/PR. |
| Quality/testing | `docs/development/testing.md`, `docs/tests/` | Testcase/Verify bổ sung test case cụ thể. |

## G1 — Project Plan Approved

- [x] Scope, constraint, dependency và architecture direction đã được ghi nhận.
- [x] Assumption/risk có owner, status và evidence/mitigation tương ứng.
- [x] Cả 13 yêu cầu Lead được trace tới scope/planning và output Phase 3.
- [x] Capability `Current` được tách khỏi experiment/capability `Planned`.
- [x] Không tạo Spec Function, Basic Design hoặc testcase chi tiết trong Phase 0–2.
- [ ] Lead review và sign-off G1.
