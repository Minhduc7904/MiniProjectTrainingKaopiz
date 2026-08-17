# Testcase — Non-functional Requirements

## Nguyên tắc bằng chứng

- Ghi commit, cấu hình máy/container, dataset/seed, warm-up, số lần chạy và raw
  result.
- Report median cùng min/max hoặc percentile phù hợp; không chỉ ghi một lần chạy.
- Performance comparison dùng cùng request, dataset và configuration.
- Chưa có SLO thì pass nghĩa là phép đo tái lập và requirement kiến trúc
  (stream/chunk/no duplicate/no leak) được chứng minh, không phải đạt con số tự
  đặt.

| TC ID | Trace | Setup / Dataset | Thao tác và metrics | Pass evidence |
| --- | --- | --- | --- | --- |
| TC-NFR-ENV-001 | NFR-ENV-01 | Docker Compose clean volume và retained volume | Start stack, health check, restart; ghi container/network/volume | Service giao tiếp bằng service name; health/dependency đúng; dữ liệu cần giữ còn sau restart. |
| TC-NFR-BATCH-001 | NFR-BATCH-01 | 3k/10k/100k ACTIVE Student; cấu hình chunk/concurrency cố định | Chạy batch ≥3 lần/dataset; time, peak memory, throughput, success/failure | Có raw report tái lập; counter khớp item; không OOM. |
| TC-NFR-BATCH-002 | NFR-BATCH-02 | 100k recipient | Profile HTTP + worker memory; inspect page/chunk | HTTP không tạo/load toàn recipient; worker bounded theo page/chunk đã công bố. |
| TC-NFR-CSV-001 | NFR-CSV-01 | 10k/100k/300k Course | So sánh materialize baseline và streaming; total time, TTFB, peak memory, size | 100k+ dùng streaming/chunking; output đầy đủ; memory không tăng tuyến tính theo CSV string toàn bộ. |
| TC-NFR-QUERY-001 | NFR-QUERY-01 | Course có nhiều Lesson/Progress | Gọi detail/list; đếm SQL và response time trước/sau | Lưu query count; không phát một query riêng cho từng Course/Lesson ngoài design. |
| TC-NFR-INDEX-001 | NFR-INDEX-01 | 10k/100k/1M Course cùng distribution | Chạy query trước/sau index; time, rows examined, selectivity | Report chỉ rõ index có/không có lợi; không giữ index vô ích chỉ để đạt checklist. |
| TC-NFR-PLAN-001 | NFR-PLAN-01 | Query của N+1/index experiment | `EXPLAIN`/`EXPLAIN ANALYZE` trước/sau | Lưu plan, access type, selected index, rows và bottleneck/kết luận. |
| TC-NFR-PAGING-001 | NFR-PAGING-01 | 100k+ row; page size cố định | Offset tăng dần và cursor tương đương; latency/rows examined | Stable ordering, không trùng/mất; report trade-off theo độ sâu. |
| TC-NFR-API-001 | NFR-API-01 | 10k/100k Course/Student | Benchmark request đại diện trước/sau; response time, throughput, SQL count, memory | Cùng environment/input; response correctness giữ nguyên; raw metrics được lưu. |
| TC-NFR-REL-001 | NFR-REL-01 | Redeliver snapshot/dispatch/media/scheduler command | Consume lặp và query DB/outbox | Không recipient/inbox/usage/run/message business trùng. |
| TC-NFR-REL-002 | NFR-REL-02 | Hai worker; lease active/expired; stale token | Claim và ghi concurrent | Claim disjoint; expired lease reclaim được; stale token không update. |
| TC-NFR-SEC-001 | NFR-SEC-01 | Actor owner/non-owner; media tồn tại/không tồn tại | Gọi resource/storage-related API | Non-owner bị chặn; response/log không lộ bucket, object key, credential hoặc dữ liệu actor khác. |
| TC-NFR-OBS-001 | NFR-OBS-01 | Dependency/batch item failure | Gửi correlation ID, gây lỗi có kiểm soát, tra log/summary | Trace xuyên API/message; có safe code/status/counter; không log secret/payload nhạy cảm. |
| TC-NFR-MAINT-001 | NFR-MAINT-01 | Architecture/schema review | Scan FK/query/connection ownership | Không cross-database FK/query; service chỉ scaffold table thuộc owner. |
| TC-NFR-ERROR-001 | NFR-ERROR-01 | Validation, not-found, conflict, DB/dependency unavailable | Gọi API representative mỗi service | Status/error envelope/traceId nhất quán; validation trước side effect; không lộ exception/SQL/secret. |

## Output report tối thiểu

```text
Testcase ID:
Commit:
Environment/configuration:
Dataset/seed:
Warm-up và số lần chạy:
Command/request:
Raw results:
Summary:
Pass/Fail theo requirement:
Known limitation:
```

