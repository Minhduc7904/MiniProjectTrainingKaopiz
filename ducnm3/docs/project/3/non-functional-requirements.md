# Non-functional Requirements

## Môi trường và container

| ID | Requirement | Tiêu chí nghiệm thu |
| --- | --- | --- |
| NFR-ENV-01 | Toàn bộ local stack phải khởi động bằng Docker Compose, gồm Gateway, năm API service, Media/Notification/Scheduler Worker, MySQL, MinIO và RabbitMQ. | Compose khai báo rõ network, volume và environment/config; các container giao tiếp bằng service name; health/dependency startup được kiểm tra; dữ liệu cần giữ lại còn tồn tại sau khi container restart. |

## Hiệu năng và khả năng mở rộng

| ID | Requirement | Tiêu chí nghiệm thu |
| --- | --- | --- |
| NFR-BATCH-01 | Notification batch phải được benchmark tại 3k, 10k và 100k recipient. | Ghi execution time, peak memory, success/failure count và throughput cho từng dataset, cùng máy/cấu hình và số lần chạy. |
| NFR-BATCH-02 | Batch phải xử lý theo page/chunk, không giữ toàn bộ recipient trong HTTP request hoặc memory. | Review evidence cho page/chunk; benchmark ghi batch size và concurrency. |
| NFR-CSV-01 | CSV export phải benchmark ở 10k, 100k và 300k record; 100k+ không được yêu cầu materialize toàn bộ record/CSV string. | So sánh baseline với streaming/chunking; ghi total time, peak memory, output size và TTFB nếu đo được. |
| NFR-QUERY-01 | Course detail/list phải đánh giá N+1 trước và sau tối ưu. | Ghi số SQL query và response time với dataset được công bố. |
| NFR-INDEX-01 | Index experiment phải chạy tại 10k, 100k và 1M Course. | Ghi index được thử, độ chọn lọc, thời gian và số hàng đã xét trước/sau; kết luận trường hợp index có hoặc không có lợi. |
| NFR-PLAN-01 | Query dùng trong index/N+1 experiment phải được phân tích bằng `EXPLAIN` hoặc `EXPLAIN ANALYZE`. | Lưu execution plan trước/sau, access type, index được chọn, rows examined và bottleneck được xác định. |
| NFR-PAGING-01 | List API dataset lớn phải đánh giá offset/cursor và độ trễ khi offset tăng. | Ghi dataset, query pattern, page size và kết quả đo. |
| NFR-API-01 | API Course list/detail và Student list đại diện phải được benchmark với 10k và 100k record trước và sau optimization. | Cùng request, dataset, máy và configuration; ghi response time, throughput nếu đo được, số SQL query và thay đổi peak memory; không công bố target giả khi chưa có SLO. |

Không đặt target millisecond giả khi lead chưa cung cấp SLO. Baseline, môi trường,
dataset, cấu hình, warm-up và số lần chạy là bắt buộc để kết quả tái lập.

## Tin cậy, bảo mật và quan sát

| ID | Requirement | Tiêu chí nghiệm thu |
| --- | --- | --- |
| NFR-REL-01 | Command bất đồng bộ phải durable và consumer idempotent. | Redelivery không tạo recipient, inbox hoặc media usage trùng. |
| NFR-REL-02 | Batch item đang xử lý phải có lease và chỉ worker có token hợp lệ được ghi kết quả. | Case lease hết hạn/token sai không làm sai trạng thái item. |
| NFR-SEC-01 | Actor chỉ truy cập resource được phép; object location không lộ cho client. | Negative authorization và response review pass. |
| NFR-OBS-01 | Lỗi dependency và batch phải traceable bằng mã lỗi/trạng thái/counter an toàn. | API/worker logs và summary cho phép xác định batch/item lỗi mà không lộ secret. |
| NFR-MAINT-01 | Service giữ database ownership; không foreign key/query xuyên database. | Design review và integration boundary review pass. |

## API error handling

| ID | Requirement | Tiêu chí nghiệm thu |
| --- | --- | --- |
| NFR-ERROR-01 | Mọi API phải xử lý nhất quán validation error, resource not found và database/dependency error. | Validation error bị từ chối trước side effect; resource không tồn tại không trả dữ liệu khác; database/dependency error được chuyển thành nhóm response phù hợp; response dùng error envelope và trace/correlation ID chung, không lộ exception, SQL hoặc secret. |

Các HTTP status/error code cụ thể của từng endpoint được chốt ở Phase 4. Phase 3
chỉ khóa semantics và tính nhất quán có thể kiểm thử.
